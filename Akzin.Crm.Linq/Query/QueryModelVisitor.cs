// Copyright (c) Akzin.com, to@fik.email
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Akzin.Crm.Linq.Select;
using Akzin.Crm.Linq.Where;
using Microsoft.Xrm.Sdk.Query;
using Akzin.Crm.Linq.Relinq;
using Akzin.Crm.Linq.Relinq.Clauses;
using Akzin.Crm.Linq.Relinq.Clauses.Expressions;
using Akzin.Crm.Linq.Relinq.Clauses.ResultOperators;

namespace Akzin.Crm.Linq.Query
{
    internal class QueryModelVisitor : QueryModelVisitorBase
    {
        private readonly Dictionary<IQuerySource, LinkEntityOrQueryExpression> linkEntities = new Dictionary<IQuerySource, LinkEntityOrQueryExpression>();
        private string entityName;

        public QueryModelVisitor(string entityName)
        {
            this.entityName = entityName;
        }

        public Projector Projector { get; } = new Projector();

        public QueryExpression QueryExpression { get; } = new QueryExpression();        

        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            linkEntities[fromClause] = new LinkEntityOrQueryExpression(QueryExpression);
            QueryExpression.EntityName = entityName ?? fromClause.ItemType.GetEntityName();
            base.VisitMainFromClause(fromClause, queryModel);
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            base.VisitWhereClause(whereClause, queryModel, index);
            var expressionTreeVisitor = new FilterExpressionTreeVisitor();
            var criteria = expressionTreeVisitor.ExtractFilterExpression(whereClause.Predicate);

            if (QueryExpression.Criteria.Conditions.Count == 0 && QueryExpression.Criteria.Filters.Count == 0)
            {
                QueryExpression.Criteria = criteria;
            }
            else
            {
                QueryExpression.Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Filters = { QueryExpression.Criteria, criteria }
                };
            }
        }

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            foreach (var orderItem in orderByClause.Orderings)
            {
                var memberExpression = (MemberExpression)orderItem.Expression;
                var alias = memberExpression.GetAttributeAlias();

                var orderExpression = new OrderExpression(alias.AttributeName,
                    orderItem.OrderingDirection == OrderingDirection.Asc
                        ? OrderType.Ascending
                        : OrderType.Descending);

                linkEntities[alias.QuerySource].AddOrder(orderExpression);
            }
            base.VisitOrderByClause(orderByClause, queryModel, index);
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            Projector.VisitSelectClause(selectClause.Selector);

            foreach (var argument in Projector.Arguments)
            {
                var argumentAlias = argument.AttributeAlias;

                if (argumentAlias.AttributeName != null)
                {
                    linkEntities[argumentAlias.QuerySource].AddColumn(argumentAlias.AttributeName);
                }
                else
                {
                    linkEntities[argumentAlias.QuerySource].SetAllColumns();
                }
            }

            base.VisitSelectClause(selectClause, queryModel);
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            var innerKeySelector = joinClause.InnerKeySelector;
            var outerKeySelector = joinClause.OuterKeySelector;

            var innerKeyAttributeAlias = innerKeySelector.GetAttributeAlias();
            var outerKeyAttributeAlias = outerKeySelector.GetAttributeAlias();
            var alias = joinClause.ItemName;

            if(outerKeyAttributeAlias.AttributeName == NamingExtensions.SpecialId || innerKeyAttributeAlias.AttributeName == NamingExtensions.SpecialId)
                throw new NotSupportedException("Id property does not have EntityLogicalNameAttribute");

            var linkEntity = new LinkEntity
            {
                EntityAlias = alias,
                LinkFromEntityName = linkEntities[outerKeyAttributeAlias.QuerySource].GetEntityName(),
                LinkFromAttributeName = outerKeyAttributeAlias.AttributeName,
                LinkToEntityName = innerKeyAttributeAlias.QuerySource.GetEntityName(),
                LinkToAttributeName = innerKeyAttributeAlias.AttributeName
            };

            linkEntities[outerKeyAttributeAlias.QuerySource].AddLinkEntity(linkEntity);
            linkEntities[innerKeyAttributeAlias.QuerySource] = new LinkEntityOrQueryExpression(linkEntity);
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
        {
            VisitJoinClause(joinClause, queryModel, -1);
        }


        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            base.VisitAdditionalFromClause(fromClause, queryModel, index);

            if (fromClause.FromExpression is SubQueryExpression subQueryExpression &&
                subQueryExpression.QueryModel.ResultOperators.Count == 1 &&
                subQueryExpression.QueryModel.ResultOperators[0] is DefaultIfEmptyResultOperator &&
                subQueryExpression.QueryModel.MainFromClause.FromExpression is QuerySourceReferenceExpression
                    groupSourceExpression &&
                groupSourceExpression.ReferencedQuerySource is GroupJoinClause groupJoinClause)
            {
                linkEntities[fromClause] = linkEntities[groupJoinClause.JoinClause];
                linkEntities[fromClause].SetJoinOperator(JoinOperator.LeftOuter);
                return;
            }

            throw new NotSupportedException("Grouping not supported");
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            if (resultOperator is FirstResultOperator ||
                resultOperator is AnyResultOperator)
            {
                QueryExpression.TopCount = Math.Min(QueryExpression.TopCount ?? 1, 1);
            }
            else if (resultOperator is SingleResultOperator)
            {
                QueryExpression.TopCount = Math.Min(QueryExpression.TopCount ?? 2, 2);
            }
            else if (resultOperator is TakeResultOperator)
            {
                var count = ((TakeResultOperator)resultOperator).GetConstantCount();
                QueryExpression.TopCount = Math.Min(QueryExpression.TopCount ?? count, count);
            }
            else if (resultOperator is SkipResultOperator)
            {
                QueryExpression.PageInfo.PageNumber = 2;
                QueryExpression.PageInfo.Count = ((SkipResultOperator)resultOperator).GetConstantCount();
            }
            else if (resultOperator is DistinctResultOperator)
            {
                QueryExpression.Distinct = true;
            }
            else if (resultOperator is CountResultOperator)
            {
                QueryExpression.PageInfo.ReturnTotalRecordCount = true;
                if (QueryExpression.TopCount == null)
                {
                    QueryExpression.PageInfo.Count = 1;
                    QueryExpression.PageInfo.PageNumber = 1;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        //public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        //{
        //    throw new NotSupportedException("Grouping is not supported");
        //}

        //protected override void VisitOrderings(ObservableCollection<Ordering> orderings, QueryModel queryModel, OrderByClause orderByClause)
        //{
        //    throw new NotImplementedException();
        //}

        //public override void VisitOrdering(Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override void VisitBodyClauses(ObservableCollection<IBodyClause> bodyClauses, QueryModel queryModel)
        //{
        //    throw new NotImplementedException();
        //}

        //public override void VisitQueryModel(QueryModel queryModel)
        //{
        //    throw new NotImplementedException();
        //}

        protected override void VisitResultOperators(ObservableCollection<ResultOperatorBase> resultOperators, QueryModel queryModel)
        {
            if (resultOperators.OfType<SkipResultOperator>().Count() > 1)
                throw new NotSupportedException("No more than one skip allowed");
            if (resultOperators.OfType<TakeResultOperator>().Count() > 1)
                throw new NotSupportedException("No more than one take allowed");
            if (resultOperators.OfType<DistinctResultOperator>().Count() > 1)
                throw new NotSupportedException("No more than one distinct allowed");

            base.VisitResultOperators(resultOperators, queryModel);
        }
    }
}

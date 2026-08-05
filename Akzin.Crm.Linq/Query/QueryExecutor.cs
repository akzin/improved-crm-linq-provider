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
using System.Linq;
using Akzin.Crm.Linq.Fetch;
using Microsoft.Xrm.Sdk;
using Akzin.Crm.Linq.Relinq;
using Akzin.Crm.Linq.Relinq.Clauses.ResultOperators;

namespace Akzin.Crm.Linq.Query
{
    internal class QueryExecutor : IQueryExecutor
    {
        private readonly IOrganizationService service;
        private readonly string entityName;
        private readonly QueryOptions queryOptions;

        public QueryExecutor(IOrganizationService service, string entityName, QueryOptions queryOptions)
        {
            this.service = service;
            this.entityName = entityName;
            this.queryOptions = queryOptions;
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            if (queryModel.ResultOperators.Any(x => x is CountResultOperator))
            {
                var queryModelVisitor = new QueryModelVisitor(entityName);
                queryModelVisitor.VisitQueryModel(queryModel);
                var count = new DataFetcher(service, queryModelVisitor, queryOptions).ExecuteCount();
                return (T)(object)count;
            }
            else
            {
                return ExecuteCollection<T>(queryModel).FirstOrDefault();
            }
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            var queryModelVisitor = new QueryModelVisitor(entityName);
            queryModelVisitor.VisitQueryModel(queryModel);
            
            var items = new DataFetcher(service, queryModelVisitor, queryOptions).Execute<T>();

            return returnDefaultWhenEmpty ? items.SingleOrDefault() : items.Single();
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var queryModelVisitor = new QueryModelVisitor(entityName);
            queryModelVisitor.VisitQueryModel(queryModel);

            if (queryModel.ResultOperators.Any(x => x is AnyResultOperator))
            {
                var items = new DataFetcher(service, queryModelVisitor, queryOptions).ExecuteEntities();
                return new[] {(T) (object) items.Any()};
            }
            else
            {
                var items = new DataFetcher(service, queryModelVisitor, queryOptions).Execute<T>();
                return items;

            }
        }
    }
}
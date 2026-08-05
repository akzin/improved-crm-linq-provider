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
using Akzin.Crm.Linq.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Akzin.Crm.Linq.Relinq.Clauses;

namespace Akzin.Crm.Linq.Fetch
{
    internal class DataFetcher
    {
        private readonly IOrganizationService service;
        private readonly QueryModelVisitor queryModelVisitor;
        private readonly QueryOptions queryOptions;

        public DataFetcher(IOrganizationService service, QueryModelVisitor queryModelVisitor, QueryOptions queryOptions)
        {
            this.service = service;
            this.queryModelVisitor = queryModelVisitor;
            this.queryOptions = queryOptions;
        }

        public int ExecuteCount()
        {
            var expression = queryModelVisitor.QueryExpression;
            var topCount = expression.TopCount;

            if (expression.TopCount != null && (expression.PageInfo.Count != 0 && expression.PageInfo.PageNumber != 0))
            {
                expression.TopCount = null;
            }

            var request = new RetrieveMultipleRequest { Query = expression };
            var response = (RetrieveMultipleResponse)service.Execute(request);
            var entitiesCollection = response.EntityCollection;

            return topCount == null
                ? entitiesCollection.TotalRecordCount
                : Math.Min(topCount.Value, entitiesCollection.TotalRecordCount);
        }

        public IEnumerable<Entity> ExecuteEntities()
        {
            var expression = queryModelVisitor.QueryExpression;
            int? topCount = expression.TopCount;

            if (expression.PageInfo.PageNumber == 0)
            {
                expression.PageInfo.PageNumber = 1;
            }

            if (expression.TopCount != null && (expression.PageInfo.Count != 0 && expression.PageInfo.PageNumber != 0))
            {
                topCount = expression.TopCount.Value;
                expression.TopCount = null;
            }

            Queue<Entity> fetchedEntities = new Queue<Entity>();
            bool moreRecords = true;
            int returned = 0;
            while (topCount == null || topCount != returned)
            {
                if (fetchedEntities.Count == 0)
                {
                    if(moreRecords == false)
                        yield break;

                    var request = new RetrieveMultipleRequest {Query = expression};
                    var response = (RetrieveMultipleResponse) service.Execute(request);
                    var entitiesCollection = response.EntityCollection;
                    fetchedEntities = new Queue<Entity>(entitiesCollection.Entities);
                    moreRecords = entitiesCollection.MoreRecords;
                    expression.PageInfo.PagingCookie = entitiesCollection.PagingCookie;
                    expression.PageInfo.PageNumber++;
                }

                if (fetchedEntities.Count == 0)
                {
                    yield break;
                }

                returned++;
                yield return fetchedEntities.Dequeue();
            }
        }

        public IEnumerable<T> Execute<T>()
        {
            var entities = ExecuteEntities();

            var projector = queryModelVisitor.Projector;
            foreach (var entity in entities)
            {
                var values = new List<object>();

                foreach (var argument in projector.Arguments)
                {
                    var attributeAlias = argument.AttributeAlias;
                    if (attributeAlias.AttributeName == null)
                    {
                        var value = GetConvertedEntity(entity, attributeAlias.QuerySource);
                        values.Add(value);
                    }
                    else
                    {
                        var value = entity.GetAttributeValue<object>(attributeAlias.DotNamed);

                        if (attributeAlias.AttributeName == NamingExtensions.SpecialRowVersion)
                        {
                            value = entity.RowVersion;
                        }

                        if (attributeAlias.AttributeName == NamingExtensions.SpecialId && value == null)
                        {
                            value = entity.Id;
                        }

                        if (value is AliasedValue aliasedValue)
                            value = aliasedValue.Value;
                        var convertedValue = Convert(value, argument.ArgumentType);

                        values.Add(convertedValue);
                    }
                }
                yield return (T)projector.Execute(values);
            }
        }

        private Entity GetConvertedEntity(Entity entity, IQuerySource querySource)
        {
            var alias = querySource.ItemName;
            var type = querySource.ItemType;

            if (!(Activator.CreateInstance(type) is Entity value))
                throw new NotImplementedException();

            if (querySource is MainFromClause)
            {
                value.Id = entity.Id;
                value.RowVersion = entity.RowVersion;
                value.FormattedValues.AddRange(entity.FormattedValues);
                value.RelatedEntities.AddRange(entity.RelatedEntities);

                var attributeValues = entity
                    .Attributes
                    .Where(x => x.Value is AliasedValue == false);
                foreach (var attributeValue in attributeValues)
                {
                    value.Attributes[attributeValue.Key] = attributeValue.Value;
                }

                queryOptions?.OnEntityCreated?.Invoke(value);

                return value.Attributes.Any() ? value : null;
            }
            else if (querySource is JoinClause)
            {
                var attributeValues = entity
                    .Attributes
                    .Where(att => att.Key.StartsWith($"{alias}."))
                    .Select(x => x.Value).Cast<AliasedValue>();

                var idAttributeName = type.GetProperty("Id").GetAttributeName();
                foreach (var attributeValue in attributeValues)
                {
                    value.Attributes[attributeValue.AttributeLogicalName] = attributeValue.Value;
                    if (idAttributeName == attributeValue.AttributeLogicalName)
                    {
                        value.Id = (Guid)attributeValue.Value;
                    }
                }

                queryOptions?.OnEntityCreated?.Invoke(value);

                return value.Attributes.Any() ? value : null;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private object Convert(object obj, Type type)
        {
            if (obj == null)
                return null;

            if (type.IsInstanceOfType(obj))
                return obj;

            if (obj is OptionSetValue optionSetValue)
            {
                if (type == typeof(int) || type == typeof(int?))
                {
                    return optionSetValue.Value;
                }

                var nullable = Nullable.GetUnderlyingType(type);
                if (nullable != null && nullable.IsEnum)
                {
                    var convertedEnum = Enum.ToObject(nullable, optionSetValue.Value);
                    return convertedEnum;
                }
            }

            if (obj is EntityReference entityReference && type == typeof(Guid))
            {
                return entityReference.Id;
            }
            throw new NotImplementedException();
        }
    }
}

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

using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk;
using Akzin.Crm.Linq.Relinq;
using Akzin.Crm.Linq.Relinq.Parsing.Structure;

namespace Akzin.Crm.Linq.Query
{
    internal class Queryable<T> : QueryableBase<T>, IEntityNamedQueryable
    {
        public string EntityName { get; }

        public Queryable(IOrganizationService service, string entityName, QueryOptions queryOptions) : base(QueryParser.CreateDefault(), new QueryExecutor(service, entityName, queryOptions))
        {
            EntityName = entityName;
        }

        // ReSharper disable once UnusedMember.Global
        public Queryable(IQueryProvider provider, Expression expression) : base(provider, expression)
        {
        }
    }
}
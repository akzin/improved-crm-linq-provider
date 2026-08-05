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
using Akzin.Crm.Linq.Relinq.Clauses;

namespace Akzin.Crm.Linq.Select
{
    internal class AttributeAlias
    {
        public AttributeAlias(IQuerySource referencedQuerySource, string attributeName)
        {
            QuerySource = referencedQuerySource;
            AttributeName = attributeName;
        }

        public string AttributeName { get; }
        public IQuerySource QuerySource { get; }


        public string JoinedSourceName
        {
            get
            {
                if (QuerySource is MainFromClause)
                {
                    return null;
                }
                else if (QuerySource is JoinClause)
                {
                    return QuerySource.ItemName;
                }
                else if (QuerySource is AdditionalFromClause)
                {
                    return QuerySource.ItemName;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public string DotNamed
        {
            get
            {
                if (QuerySource is MainFromClause)
                {
                    return AttributeName;
                }
                else if (QuerySource is JoinClause)
                {
                    return $"{QuerySource.ItemName}.{AttributeName}";
                }
                else if (QuerySource is AdditionalFromClause)
                {
                    return $"{QuerySource.ItemName}.{AttributeName}";
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
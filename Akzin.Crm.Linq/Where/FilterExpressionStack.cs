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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;

namespace Akzin.Crm.Linq.Where
{
    internal partial class FilterExpressionTreeVisitor
    {
        class FilterExpressionStack
        {
            private readonly Stack<FilterExpression> criteriaStack = new Stack<FilterExpression>();
            public FilterExpression RootCriteria { get; private set; }

            public void PushFilter(FilterExpression filter)
            {
                if (RootCriteria == null)
                {
                    RootCriteria = filter;
                    criteriaStack.Push(filter);
                }
                else
                {
                    var top = criteriaStack.Peek();
                    top.AddFilter(filter);
                    criteriaStack.Push(filter);
                }
            }

            public void PopFilter()
            {
                criteriaStack.Pop();
            }

            public void PushCondition(ConditionExpression condition)
            {
                if (RootCriteria == null)
                {
                    PushFilter(new FilterExpression());
                }
                var top = criteriaStack.Peek();
                top.AddCondition(condition);
            }

            public ConditionExpression PeekCondition()
            {
                var top = criteriaStack.Peek();
                var lastCondition = top.Conditions.Last();
                return lastCondition;
            }

            public void Collapse()
            {
                if (RootCriteria != null)
                {
                    Collapse(RootCriteria);
                }
            }

            private void Collapse(FilterExpression parent)
            {
                foreach (var child in parent.Filters.ToArray())
                {
                    Collapse(child);

                    if (child.FilterOperator == parent.FilterOperator)
                    {
                        parent.Conditions.AddRange(child.Conditions);
                        parent.Filters.AddRange(child.Filters);
                        parent.Filters.Remove(child);
                    }
                }
            }
        }
    }
}
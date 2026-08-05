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
using System.Linq.Expressions;
using System.Reflection;
using Akzin.Crm.Linq.Relinq.Parsing.ExpressionVisitors;

namespace Akzin.Crm.Linq.Select
{
    internal class Projector
    {
        // ReSharper disable once PossibleNullReferenceException
        private static readonly MethodInfo ValueMethodInfo = typeof(SelectClauseArgument).GetProperty(nameof(SelectClauseArgument.Value)).GetMethod;
        
        private Expression projection;

        public List<SelectClauseArgument> Arguments {get; } = new List<SelectClauseArgument>();

        public void VisitSelectClause(Expression selectClause)
        {
            var convertedSelectClause = new SelectClauseVisitor(this).Visit(selectClause);
            projection = convertedSelectClause;
        }

        public object Execute(List<object> arguments)
        {
            if(arguments.Count != Arguments.Count)
                throw new NotImplementedException();

            for (int i = 0; i < arguments.Count; i++)
            {
                Arguments[i].Value = arguments[i];
            }

            return SandboxExpressionEvaluator.Evaluate(projection);
        }

        public UnaryExpression AddArgument(AttributeAlias alias, Type argumentType)
        {
            var argument = new SelectClauseArgument
            {
                AttributeAlias = alias,
                ArgumentType = argumentType
            };

            Arguments.Add(argument);

            var callPropertyGetValue = Expression.Call(Expression.Constant(argument), ValueMethodInfo);
            var convertValue = Expression.Convert(callPropertyGetValue, argumentType);
            return convertValue;
        }
    }
}

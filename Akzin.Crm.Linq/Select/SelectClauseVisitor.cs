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
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk;
using Akzin.Crm.Linq.Relinq.Clauses.Expressions;
using Akzin.Crm.Linq.Relinq.Parsing;

namespace Akzin.Crm.Linq.Select
{
    internal class SelectClauseVisitor : RelinqExpressionVisitor
    {
        private readonly Projector projector;

        public SelectClauseVisitor(Projector projector)
        {
            this.projector = projector;
        }

        protected internal override Expression VisitSubQuery(SubQueryExpression expression)
        {
            throw new NotSupportedException("Sub-queries are not supported");
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if (typeof(Entity).IsAssignableFrom(expression.Method.DeclaringType))
            {
                if (expression.Method.Name == "GetAttributeValue")
                {
                    var type = expression.Type;
                    var alias = expression.GetAttributeAlias();
                    var expressionReplacement = projector.AddArgument(alias, type);
                    return expressionReplacement;
                }
                throw new NotSupportedException($"Method not supported: {expression.Method.Name} on {expression.Type.FullName}");
            }
            else
            {
                return base.VisitMethodCall(expression);
            }
        }

        protected internal override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
        {
            var type = expression.Type;
            var alias = expression.GetAttributeAlias();
            var expressionReplacement = projector.AddArgument(alias, type);
            return expressionReplacement;
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            if (expression.Expression is QuerySourceReferenceExpression)
            {
                var type = expression.Type;
                var alias = expression.GetAttributeAlias();
                var expressionReplacement = projector.AddArgument(alias, type);
                return expressionReplacement;
            }
            else
            {
                return base.VisitMember(expression);
            }
        }
    }
}

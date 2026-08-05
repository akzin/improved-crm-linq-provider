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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Akzin.Crm.Linq.Relinq.Clauses.Expressions;
using Akzin.Crm.Linq.Relinq.Clauses.ResultOperators;
using Akzin.Crm.Linq.Relinq.Parsing;

namespace Akzin.Crm.Linq.Where
{
    internal partial class FilterExpressionTreeVisitor : ThrowingExpressionVisitor
    {
        private FilterExpressionStack stack;

        public FilterExpression ExtractFilterExpression(Expression expression)
        {
            stack = new FilterExpressionStack();
            Visit(expression);

            stack.Collapse();
            return stack.RootCriteria;
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    AddCondition(expression.Left, ConditionOperator.Equal, expression.Right);
                    return expression;
                case ExpressionType.NotEqual:
                    AddCondition(expression.Left, ConditionOperator.NotEqual, expression.Right);
                    return expression;
                case ExpressionType.GreaterThan:
                    AddCondition(expression.Left, ConditionOperator.GreaterThan, expression.Right);
                    return expression;
                case ExpressionType.GreaterThanOrEqual:
                    AddCondition(expression.Left, ConditionOperator.GreaterEqual, expression.Right);
                    return expression;
                case ExpressionType.LessThan:
                    AddCondition(expression.Left, ConditionOperator.LessThan, expression.Right);
                    return expression;
                case ExpressionType.LessThanOrEqual:
                    AddCondition(expression.Left, ConditionOperator.LessEqual, expression.Right);
                    return expression;
                case ExpressionType.AndAlso:
                    stack.PushFilter(new FilterExpression(LogicalOperator.And));
                    Visit(expression.Left);
                    Visit(expression.Right);
                    stack.PopFilter();
                    return expression;
                case ExpressionType.OrElse:
                    stack.PushFilter(new FilterExpression(LogicalOperator.Or));
                    Visit(expression.Left);
                    Visit(expression.Right);
                    stack.PopFilter();
                    return expression;
                default:
                    return base.VisitBinary(expression);
            }
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Not)
            {
                if(expression.Operand is BinaryExpression binaryExpression && (binaryExpression.NodeType == ExpressionType.AndAlso || binaryExpression.NodeType == ExpressionType.OrElse))
                    throw new NotSupportedException("! is not supported in grouping");

                Visit(expression.Operand);
                var condition = stack.PeekCondition();

                switch (condition.Operator)
                {
                    case ConditionOperator.Like:
                        condition.Operator = ConditionOperator.NotLike;
                        return expression;
                    case ConditionOperator.Equal:
                        condition.Operator = ConditionOperator.NotEqual;
                        return expression;
                    case ConditionOperator.NotEqual:
                        condition.Operator = ConditionOperator.Equal;
                        return expression;
                    case ConditionOperator.Null:
                        condition.Operator = ConditionOperator.NotNull;
                        return expression;
                    case ConditionOperator.NotNull:
                        condition.Operator = ConditionOperator.Null;
                        return expression;
                    case ConditionOperator.In:
                        condition.Operator = ConditionOperator.NotIn;
                        return expression;
                    case ConditionOperator.NotIn:
                        condition.Operator = ConditionOperator.In;
                        return expression;
                    default: 
                        throw new NotSupportedException();
                }
            }
            else
            {
                return base.VisitUnary(expression);
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.Name == "Equals" && expression.Method.GetParameters().Length == 1)
            {
                AddCondition(expression.Object, ConditionOperator.Equal, expression.Arguments[0]);
                return expression;
            }
            else if (expression.Method.DeclaringType == typeof(string) &&
                     expression.Method.Name == "Contains" &&
                     expression.Method.GetParameters().Length == 1 &&
                     expression.Method.GetParameters()[0].ParameterType == typeof(string))
            {
                var argument = expression.Arguments[0] as ConstantExpression;
                if (argument == null)
                    throw new NotSupportedException("String.Contains requires a constant search value");

                var searchValue = (string)argument.Value;
                if (searchValue == null)
                    throw new ArgumentNullException("value");

                AddCondition(
                    expression.Object,
                    ConditionOperator.Like,
                    Expression.Constant(ToContainsPattern(searchValue)));
                return expression;
            }
            else if (expression.Type == typeof(bool))
            {
                AddCondition(expression, ConditionOperator.Equal, Expression.Constant(true));
                return expression;
            }
            else
            {
                return base.VisitMethodCall(expression);
            }
        }

        protected internal override Expression VisitSubQuery(SubQueryExpression expression)
        {
            var operators = expression.QueryModel.ResultOperators;
            if (operators.Count == 1 && operators[0] is ContainsResultOperator containsResultOperator)
            {
                var left = containsResultOperator.Item;
                var right = expression.QueryModel.MainFromClause.FromExpression;
                AddCondition(left, ConditionOperator.In, right);
                return expression;
            }
            return base.VisitSubQuery(expression);
        }

        private void AddCondition(Expression leftExpression, ConditionOperator op, Expression rightExpression)
        {
            if (leftExpression is ConstantExpression && !(rightExpression is ConstantExpression))
            {
                var temporary = leftExpression;
                leftExpression = rightExpression;
                rightExpression = temporary;
                op = ReverseComparison(op);
            }

            var condition = new ConditionExpression { Operator = op };

            var left = leftExpression.GetAttributeAlias();
            condition.EntityName = left.JoinedSourceName;
            condition.AttributeName = left.AttributeName;

            if (rightExpression is ConstantExpression right)
            {
                var rightValue = ConvertConstant(right.Value);
                if (rightValue == null && (condition.Operator == ConditionOperator.Equal ||
                                           condition.Operator == ConditionOperator.NotEqual))
                {
                    condition.Operator = condition.Operator == ConditionOperator.Equal
                        ? ConditionOperator.Null
                        : ConditionOperator.NotNull;
                }
                else if (rightValue is IEnumerable<object>)
                {
                    condition.Values.AddRange((IEnumerable<object>)rightValue);
                }
                else
                {
                    condition.Values.Add(rightValue);
                }
            }
            else
            {
                throw new NotSupportedException("Right hand side must be a constant");
            }

            stack.PushCondition(condition);
        }

        private static ConditionOperator ReverseComparison(ConditionOperator op)
        {
            switch (op)
            {
                case ConditionOperator.Equal:
                case ConditionOperator.NotEqual:
                    return op;
                case ConditionOperator.GreaterThan:
                    return ConditionOperator.LessThan;
                case ConditionOperator.GreaterEqual:
                    return ConditionOperator.LessEqual;
                case ConditionOperator.LessThan:
                    return ConditionOperator.GreaterThan;
                case ConditionOperator.LessEqual:
                    return ConditionOperator.GreaterEqual;
                default:
                    throw new NotSupportedException("Cannot reverse condition operator " + op);
            }
        }

        private object ConvertConstant(object value)
        {
            if (value == null)
                return null;

            var optionSetValue = value as OptionSetValue;
            if (optionSetValue != null)
                return optionSetValue.Value;

            var entityReference = value as EntityReference;
            if (entityReference != null)
                return entityReference.Id;

            if (value is string || value is int || value is DateTime || value is Guid || value is bool || value is float || value is double || value is decimal || value is long)
                return value;

            var enumerable = value as IEnumerable;
            if (enumerable != null)
                return enumerable.Cast<object>().ToArray();

            throw new NotImplementedException();
        }

        private static string ToContainsPattern(string value)
        {
            // QueryExpression does not expose SQL's ESCAPE clause. Dataverse supports
            // SQL-style bracket literals in Like patterns, so escape user wildcards
            // before adding the wildcards that implement string.Contains.
            var escaped = value
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("_", "[_]");

            return "%" + escaped + "%";
        }


        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            throw new NotImplementedException($"Method {visitMethod} not supported");
        }
    }
}

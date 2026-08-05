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
using System.Reflection;
using Akzin.Crm.Linq.Query;
using Akzin.Crm.Linq.Select;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Akzin.Crm.Linq.Relinq.Clauses;
using Akzin.Crm.Linq.Relinq.Clauses.Expressions;

namespace Akzin.Crm.Linq
{
    internal static class NamingExtensions
    {
        public const string SpecialRowVersion = "Special_RowVersion";
        public const string SpecialId = "Special_Id";

        public static AttributeAlias GetAttributeAlias(this Expression expression)
        {
            if (expression is QuerySourceReferenceExpression querySourceReferenceExpression)
            {
                return new AttributeAlias(querySourceReferenceExpression.ReferencedQuerySource, null);
            }

            while (expression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
            {
                expression = unaryExpression.Operand;
            }

            if (expression is MemberExpression memberExpression)
            {
                // EntityReference.Id
                if (memberExpression.Expression is MemberExpression innerMemberExpression)
                {
                    memberExpression = innerMemberExpression;
                }

                // GetAttributeValue<EntityReference>("attribute").Id
                if (memberExpression.Expression is MethodCallExpression mmethodCallExpression && mmethodCallExpression.Method.Name == "GetAttributeValue")
                {
                    return GetAttributeAlias(mmethodCallExpression);
                }

                if (memberExpression.Expression is QuerySourceReferenceExpression innerQuerySourceReferenceExpression)
                {
                    var attributeName = memberExpression.GetAttributeName();
                    if (attributeName == SpecialRowVersion &&
                        innerQuerySourceReferenceExpression.ReferencedQuerySource is MainFromClause == false)
                    {
                        throw new NotSupportedException("RowVersion is only available on the main from clause");
                    }

                    return new AttributeAlias(innerQuerySourceReferenceExpression.ReferencedQuerySource, attributeName);
                }
                throw new NotImplementedException();
            }

            if (expression is MethodCallExpression methodCallExpression && methodCallExpression.Method.Name == "GetAttributeValue")
            {
                var source = (QuerySourceReferenceExpression)methodCallExpression.Object;
                var attributeName = (string)((ConstantExpression)methodCallExpression.Arguments[0]).Value;

                return new AttributeAlias(source.ReferencedQuerySource, attributeName);
            }

            throw new NotImplementedException();
        }

        public static string GetEntityName(this IQuerySource querySource)
        {
            var type = querySource.ItemType;
            var attribute = type.GetCustomAttribute<EntityLogicalNameAttribute>();
            if (attribute != null)
                return attribute.LogicalName;


            if (querySource is JoinClause joinClause)
            {
                var sequence = joinClause.InnerSequence;

                if (sequence is ConstantExpression constantExpression)
                {
                    if (constantExpression.Value is IEntityNamedQueryable queryable)
                    {
                        return queryable.EntityName;
                    }
                }
            }

            throw new NotSupportedException($"Type {type.FullName} does not have EntityLogicalNameAttribute");
        }

        public static string GetEntityName(this Type type)
        {
            var attribute = type.GetCustomAttribute<EntityLogicalNameAttribute>();
            if (attribute == null)
                throw new NotSupportedException($"Type {type.FullName} does not have EntityLogicalNameAttribute");

            return attribute.LogicalName;
        }

        public static string GetAttributeName(this MemberInfo member)
        {
            var attributeLogicalName = member.GetCustomAttribute<AttributeLogicalNameAttribute>();
            if (attributeLogicalName == null)
            {
                if (member.Name == "RowVersion")
                {
                    return SpecialRowVersion;
                }

                if (member.Name == "Id")
                {
                    return SpecialId;
                }

                throw new NotSupportedException($"Member {member.Name} does not have AttributeLogicalNameAttribute");
            }
            return attributeLogicalName.LogicalName;
        }

        public static string GetAttributeName(this MemberExpression memberExpression)
        {
            var memberInfo = GetMemberInfo(memberExpression);
            return GetAttributeName(memberInfo);
        }

        private static MemberInfo GetMemberInfo(this MemberExpression memberExpression)
        {
            if (memberExpression.Expression is QuerySourceReferenceExpression querySourceReferenceExpression)
            {
                var propertyName = memberExpression.Member.Name;
                var type = querySourceReferenceExpression.Type;
                var propertyInfo = type.GetProperty(propertyName);
                return propertyInfo;
            }
            else if (memberExpression.Expression is MemberExpression innerMemberExpression)
            {
                return innerMemberExpression.Member;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
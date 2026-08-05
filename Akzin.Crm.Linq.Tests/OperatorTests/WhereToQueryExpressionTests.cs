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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Akzin.Crm.Linq.Tests.OperatorTests
{
    [TestClass]
    public class WhereToQueryExpressionTests
    {
        [TestMethod]
        public void DateGreaterThan()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.CreatedOn > new DateTime(2010, 11, 12)
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("createdon", ConditionOperator.GreaterThan, new DateTime(2010, 11, 12)) }
                }
            });
        }

        [TestMethod]
        public void DateGreaterEqual()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.CreatedOn >= new DateTime(2010, 11, 12)
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("createdon", ConditionOperator.GreaterEqual, new DateTime(2010, 11, 12)) }
                }
            });
        }

        [TestMethod]
        public void DateLessThan()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.CreatedOn < new DateTime(2010, 11, 12)
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("createdon", ConditionOperator.LessThan, new DateTime(2010, 11, 12)) }
                }
            });
        }

        [TestMethod]
        public void DateLessEqual()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.CreatedOn <= new DateTime(2010, 11, 12)
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("createdon", ConditionOperator.LessEqual, new DateTime(2010, 11, 12)) }
                }
            });
        }

        [TestMethod]
        public void EqualsString()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.Firstname.Equals("Firstname")
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("firstname", ConditionOperator.Equal, "Firstname") }
                }
            });
        }

        [TestMethod]
        public void LikeString()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.Firstname.Contains("Firstname")
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("firstname", ConditionOperator.Like, "%Firstname%") }
                }
            });
        }

        [TestMethod]
        public void NotLikeString()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where !c.Firstname.Contains("Firstname")
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("firstname", ConditionOperator.NotLike, "%Firstname%") }
                }
            });
        }


        [TestMethod]
        public void NotEqualsNullString()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where !(c.Firstname == null)
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("firstname", ConditionOperator.NotNull) }
                }
            });
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "PossibleUnintendedReferenceComparison")]
        public void EqualOptionSet()
        {
            var crm = new CrmMock();

            var q = from a in crm.Query<Account>()
                where a.StatusCode == new OptionSetValue(123)
                select a;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("statuscode", ConditionOperator.Equal, 123) }
                }
            });
        }

        [TestMethod]
        public void EqualOptionSetEnum()
        {
            var crm = new CrmMock();

            var q = from a in crm.Query<Account>()
                where a.StatusCodeEnum == AccountStatuscode.Active
                select a;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("statuscode", ConditionOperator.Equal, 1) }
                }
            });
        }

        [TestMethod]
        public void EqualOptionSetEnumNumber()
        {
            var crm = new CrmMock();

            var q = from a in crm.Query<Account>()
                where a.StatusCodeEnum == (AccountStatuscode)789
                select a;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("statuscode", ConditionOperator.Equal, 789) }
                }
            });
        }

        [TestMethod]
        public void EqualOptionSetValue()
        {
            var crm = new CrmMock();

            var q = from a in crm.Query<Account>()
                where a.StatusCode.Value == 987
                select a;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("statuscode", ConditionOperator.Equal, 987) }
                }
            });
        }


        [TestMethod]
        [ExpectedException(typeof(NotSupportedException), "! is not supported in grouping")]
        public void NotGroupedTwoEquals()
        {
            var crm = new CrmMock();

            var q = from a in crm.Query<Account>()
                where !(a.Name == "John" && a.Name == "Pete")
                select a;
            q.ToList();
        }

        [TestMethod]
        public void StringContainsAddsWildcardsToLikeCondition()
        {
            AssertContainsPattern("needle", "%needle%");
        }

        [TestMethod]
        public void StringContainsEscapesPercentWildcard()
        {
            AssertContainsPattern("10% discount", "%10[%] discount%");
        }

        [TestMethod]
        public void StringContainsEscapesSingleCharacterWildcard()
        {
            AssertContainsPattern("a_b", "%a[_]b%");
        }

        [TestMethod]
        public void StringContainsEscapesOpeningBracketWildcard()
        {
            AssertContainsPattern("[abc]", "%[[]abc]%");
        }

        [TestMethod]
        public void StringContainsEscapesCombinedWildcardCharacters()
        {
            AssertContainsPattern("[%_]", "%[[][%][_]]%");
        }

        [TestMethod]
        public void StringContainsEmptyStringMatchesEveryNonNullString()
        {
            AssertContainsPattern(string.Empty, "%%");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StringContainsNullThrowsArgumentNullException()
        {
            AssertContainsPattern(null, null);
        }

        [TestMethod]
        public void ConstantOnLeftSideOfComparisonIsNormalized()
        {
            var crm = new CrmMock();

            crm.Query<Contact>()
                .Where(contact => "Ada" == contact.Firstname)
                .ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("firstname", ConditionOperator.Equal, "Ada") }
                }
            });
        }

        [TestMethod]
        public void ConstantOnLeftSideReversesRelationalOperator()
        {
            var crm = new CrmMock();
            var cutoff = new DateTime(2020, 1, 2);

            crm.Query<Account>()
                .Where(account => cutoff < account.CreatedOn)
                .ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("createdon", ConditionOperator.GreaterThan, cutoff) }
                }
            });
        }

        private static void AssertContainsPattern(string input, string expectedPattern)
        {
            var crm = new CrmMock();

            crm.Query<Contact>()
                .Where(contact => contact.Firstname.Contains(input))
                .ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("firstname", ConditionOperator.Like, expectedPattern) }
                }
            });
        }

    }
}

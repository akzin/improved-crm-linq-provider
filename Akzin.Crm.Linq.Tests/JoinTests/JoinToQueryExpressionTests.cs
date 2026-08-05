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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Akzin.Crm.Linq.Tests.JoinTests
{
    [TestClass]
    public class JoinToQueryExpressionTests
    {
        [TestMethod]
        public void JoinTwoEntitiesReturnSecond()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                select acc;

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                LinkEntities =
                {
                    new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.Inner)
                    {
                        EntityAlias = "acc",
                        Columns = new ColumnSet(true)
                    }
                }
            });
        }

        [TestMethod]
        public void JoinTwoEntitiesViaModifiedByReturnSecond()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ModifiedBy.Id equals acc.ModifiedBy.Id
                    select acc;

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                LinkEntities =
                {
                    new LinkEntity("contact", "account", "modifiedby", "modifiedby", JoinOperator.Inner)
                    {
                        EntityAlias = "acc",
                        Columns = new ColumnSet(true)
                    }
                }
            });
        }

        [TestMethod]
        public void JoinTwoEntitiesByNonId()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                    join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                    select new { a = acc, c };

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                LinkEntities =
                {
                    new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.Inner)
                    {
                        EntityAlias = "acc",
                        Columns = new ColumnSet(true)
                    }
                }
            });
        }


        [TestMethod]
        public void JoinTwoEntitiesById()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.Id
                select new { a = acc, c };

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                LinkEntities =
                {
                    new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.Inner)
                    {
                        EntityAlias = "acc",
                        Columns = new ColumnSet(true)
                    }
                }
            });
        }
    }
}

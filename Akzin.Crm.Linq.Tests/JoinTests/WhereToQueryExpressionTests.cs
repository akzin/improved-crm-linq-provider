using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Akzin.Crm.Linq.Tests.JoinTests
{
    [TestClass]
    public class WhereToQueryExpressionTests
    {
        [TestMethod]
        public void GetAllAccounts()
        {
            var crm = new CrmMock();// Copyright (c) Akzin.com, to@fik.email
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
        public void GetAccountsByContactFullName()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                    join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                    where c.Fullname == "First Person"
                    select acc;

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("fullname", ConditionOperator.Equal, "First Person") }
                },
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
        public void GetAccountsByName()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                    join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                    where acc.Name == "First Company"
                    select acc;

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("name", ConditionOperator.Equal, "First Company") { EntityName = "acc" }
                    }
                },
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
        public void GetAccountsByNameAndCreatedOn()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                    join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                    where acc.Name == "First Company" && acc.CreatedOn == new DateTime(2010, 11, 12)
                    select acc;

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("name", ConditionOperator.Equal, "First Company") { EntityName = "acc" },
                        new ConditionExpression("createdon", ConditionOperator.Equal, new DateTime(2010, 11, 12)) { EntityName = "acc" }
                    }
                },
                LinkEntities =
                {
                    new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.Inner)
                    {
                        EntityAlias = "acc",
                        Columns = new ColumnSet(true),
                        LinkCriteria = new FilterExpression(LogicalOperator.And)
                    }
                }
            });
        }

        [TestMethod]
        public void GetAccountsByNameAndContactFullName()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                    join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                    where acc.Name == "First Company" && c.Fullname == "First Person"
                    select acc;

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("name", ConditionOperator.Equal, "First Company") { EntityName = "acc" },
                        new ConditionExpression("fullname", ConditionOperator.Equal, "First Person")
                    }
                },
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
        [ExpectedException(typeof(NotSupportedException), "Right hand side must be a constant")]
        public void QueryComparingTwoAttributes()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                where acc.Name == c.Fullname
                select acc;

            q.ToList();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException), "Right hand side must be a constant")]
        public void QueryComparingTwoAttributesUsingGetAttributeValue()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                where acc.GetAttributeValue<string>("name") == acc.GetAttributeValue<string>("fullname")
                    select acc;

            q.ToList();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException), "Right hand side must be a constant")]
        public void QueryComparingTwoAttributesUsingGetAttributeValueEquals()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                where acc.GetAttributeValue<string>("name").Equals(acc.GetAttributeValue<string>("fullname"))
                select acc;

            q.ToList();
        }
    }
}

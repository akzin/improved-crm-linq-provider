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
    public class TripleJoinToQueryExpressionTests
    {
        [TestMethod]
        public void JoinThreeEntitiesAndReturnThird()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                    join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                    join su in crm.Query<SystemUser>() on acc.PreferredSystemUserId.Id equals su.Id
                    select su;

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                LinkEntities =
                {
                    new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.Inner)
                    {
                        EntityAlias = "acc",
                        LinkEntities =
                        {
                            new LinkEntity("account", "systemuser", "preferredsystemuserid", "systemuserid", JoinOperator.Inner)
                            {
                                EntityAlias = "su",
                                Columns = new ColumnSet(true)
                            }
                        }
                    }
                }
            });
        }

        [TestMethod]
        public void JoinThreeEntitiesAndReturnThirdAndSecond()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                join su in crm.Query<SystemUser>() on acc.PreferredSystemUserId.Id equals su.Id
                select new {su, acc};

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                LinkEntities =
                {
                    new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.Inner)
                    {
                        EntityAlias = "acc",
                        Columns = new ColumnSet(true),
                        LinkEntities =
                        {
                            new LinkEntity("account", "systemuser", "preferredsystemuserid", "systemuserid", JoinOperator.Inner)
                            {
                                EntityAlias = "su",
                                Columns = new ColumnSet(true)
                            }
                        }
                    }
                }
            });
        }

        [TestMethod]
        public void JoinThreeEntitiesAndReturnPropertiesFromThirdAndSecond()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                join su in crm.Query<SystemUser>() on acc.PreferredSystemUserId.Id equals su.Id
                select new { su.Firstname, su.Lastname, acc.Name };

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                LinkEntities =
                {
                    new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.Inner)
                    {
                        EntityAlias = "acc",
                        Columns = new ColumnSet("name"),
                        LinkEntities =
                        {
                            new LinkEntity("account", "systemuser", "preferredsystemuserid", "systemuserid", JoinOperator.Inner)
                            {
                                EntityAlias = "su",
                                Columns = new ColumnSet("firstname", "lastname")
                            }
                        }
                    }
                }
            });
        }

        [TestMethod]
        public void JoinThreeEntitiesAndReturnAll()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                join su in crm.Query<SystemUser>() on acc.PreferredSystemUserId.Id equals su.Id
                select new { su, acc, c };

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                LinkEntities =
                {
                    new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.Inner)
                    {
                        EntityAlias = "acc",
                        Columns = new ColumnSet(true),
                        LinkEntities =
                        {
                            new LinkEntity("account", "systemuser", "preferredsystemuserid", "systemuserid", JoinOperator.Inner)
                            {
                                EntityAlias = "su",
                                Columns = new ColumnSet(true)
                            }
                        }
                    }
                }
            });
        }

        [TestMethod]
        public void JoinThreeEntitiesAndReturnPropertiesFromThirdAndSecondAndFirstEntity()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                join su in crm.Query<SystemUser>() on acc.PreferredSystemUserId.Id equals su.Id
                select new { su.Firstname, su.Lastname, acc.Name, c };

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                LinkEntities =
                {
                    new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.Inner)
                    {
                        EntityAlias = "acc",
                        Columns = new ColumnSet("name"),
                        LinkEntities =
                        {
                            new LinkEntity("account", "systemuser", "preferredsystemuserid", "systemuserid", JoinOperator.Inner)
                            {
                                EntityAlias = "su",
                                Columns = new ColumnSet("firstname", "lastname")
                            }
                        }
                    }
                }
            });
        }

        [TestMethod]
        public void JoinThreeEntitiesAndReturnPropertiesFromFirstAndSecondAndSecondEntity()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                join su in crm.Query<SystemUser>() on acc.PreferredSystemUserId.Id equals su.Id
                select new { su.Firstname, su.Lastname, c.GenderCode, acc};

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("gendercode"),
                LinkEntities =
                {
                    new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.Inner)
                    {
                        EntityAlias = "acc",
                        Columns = new ColumnSet(true),
                        LinkEntities =
                        {
                            new LinkEntity("account", "systemuser", "preferredsystemuserid", "systemuserid", JoinOperator.Inner)
                            {
                                EntityAlias = "su",
                                Columns = new ColumnSet("firstname", "lastname")
                            }
                        }
                    }
                }
            });
        }
    }
}

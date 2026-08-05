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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Akzin.Crm.Linq.Tests.FilterCombinations
{
    [TestClass]
    public class WhereToQueryExpressionTests
    {
        

        [TestMethod]
        public void DateGreaterThanAndLessThan()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.CreatedOn > new DateTime(2010, 11, 12) && c.CreatedOn < new DateTime(2010, 11, 12)
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("createdon", ConditionOperator.GreaterThan, new DateTime(2010, 11, 12)),
                        new ConditionExpression("createdon", ConditionOperator.LessThan, new DateTime(2010, 11, 12))
                    }
                }
            });
        }

        [TestMethod]
        public void DateGreaterThanOrLessThan()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.CreatedOn > new DateTime(2010, 11, 12) || c.CreatedOn < new DateTime(2010, 11, 12)
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.Or)
                {
                    Conditions =
                    {
                        new ConditionExpression("createdon", ConditionOperator.GreaterThan, new DateTime(2010, 11, 12)),
                        new ConditionExpression("createdon", ConditionOperator.LessThan, new DateTime(2010, 11, 12))
                    }
                }
            });
        }


        [TestMethod]
        public void AoBnC()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.CreatedOn > new DateTime(2010, 11, 12) || (c.CreatedOn < new DateTime(2010, 11, 12) && c.CreatedOn == new DateTime(2010, 11, 12))
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.Or)
                {
                    Conditions = { new ConditionExpression("createdon", ConditionOperator.GreaterThan, new DateTime(2010, 11, 12)) },
                    Filters = { new FilterExpression(LogicalOperator.And)
                    {
                        Conditions =
                        {
                            new ConditionExpression("createdon", ConditionOperator.LessThan, new DateTime(2010, 11, 12)),
                            new ConditionExpression("createdon", ConditionOperator.Equal, new DateTime(2010, 11, 12))
                        }
                    }}
                }
            });
        }

        [TestMethod]
        public void AnBTwoWheres()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.CreatedOn > new DateTime(2010, 11, 12)
                where c.CreatedOn < new DateTime(2010, 11, 12)
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Filters =
                    {
                        new FilterExpression(LogicalOperator.And)
                        {
                            Conditions =
                            {
                                new ConditionExpression("createdon", ConditionOperator.GreaterThan, new DateTime(2010, 11, 12)),
                            }
                        },
                        new FilterExpression(LogicalOperator.And)
                        {
                            Conditions =
                            {
                                new ConditionExpression("createdon", ConditionOperator.LessThan, new DateTime(2010, 11, 12))
                            }
                        }
                    }
                }
            });
        }

        [TestMethod]
        public void AnComplex()
        {
            var crm = new CrmMock();

            var today = DateTime.Today;

            var q = from c in crm.Query<Contact>()
                where c.Firstname == "First" &&
                      ((c.DateEnd == null && c.DateStart == null) ||
                       (c.DateEnd == null && c.DateStart <= today) ||
                       (c.DateStart == null && c.DateEnd >= today) ||
                       (c.DateStart <= today && c.DateEnd >= today))
                    select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("firstname", ConditionOperator.Equal, "First")},
                    Filters =
                    {
                        new FilterExpression(LogicalOperator.Or)
                        {
                            Filters =
                            {
                                new FilterExpression(LogicalOperator.And)
                                {
                                    Conditions =
                                    {
                                        new ConditionExpression("datestart", ConditionOperator.LessEqual, today),
                                        new ConditionExpression("dateend", ConditionOperator.GreaterEqual, today),
                                    }
                                },
                                new FilterExpression(LogicalOperator.And)
                                {
                                    Conditions =
                                    {
                                        new ConditionExpression("datestart", ConditionOperator.Null),
                                        new ConditionExpression("dateend", ConditionOperator.GreaterEqual, today),
                                    }
                                },
                                new FilterExpression(LogicalOperator.And)
                                {
                                    Conditions =
                                    {
                                        new ConditionExpression("dateend", ConditionOperator.Null),
                                        new ConditionExpression("datestart", ConditionOperator.Null),
                                    }
                                },
                                new FilterExpression(LogicalOperator.And)
                                {
                                    Conditions =
                                    {
                                        new ConditionExpression("dateend", ConditionOperator.Null),
                                        new ConditionExpression("datestart", ConditionOperator.LessEqual, today),
                                    }
                                },
                            }
                        }
                    }
                }
            });
        }

        [TestMethod]
        public void JoinAndSecondAoBnC()
        {
            var crm = new CrmMock();

            var q = from a in crm.Query<Account>()
                    join c in crm.Query<Contact>() on a.Id equals c.ParentCustomerId.Id
                    where c.CreatedOn > new DateTime(2010, 11, 12) || (c.CreatedOn < new DateTime(2010, 11, 12) && c.CreatedOn == new DateTime(2010, 11, 12))
                    select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(false),
                Criteria = new FilterExpression(LogicalOperator.Or)
                {
                    Conditions = { new ConditionExpression("createdon", ConditionOperator.GreaterThan, new DateTime(2010, 11, 12)) { EntityName = "c" } },
                    Filters = { new FilterExpression(LogicalOperator.And)
                    {
                        Conditions =
                        {
                            new ConditionExpression("createdon", ConditionOperator.LessThan, new DateTime(2010, 11, 12)) { EntityName = "c"},
                            new ConditionExpression("createdon", ConditionOperator.Equal, new DateTime(2010, 11, 12)) { EntityName = "c"}
                        }
                    }}
                },
                LinkEntities =
                {
                    new LinkEntity("account", "contact", "accountid", "parentcustomerid", JoinOperator.Inner)
                    {
                        EntityAlias = "c",
                        Columns = new ColumnSet(true)
                    }
                }
            });
        }
        
        [TestMethod]
        public void JoinAndSecondAoBnCoFirstD()
        {
            var crm = new CrmMock();

            var q = from a in crm.Query<Account>()
                join c in crm.Query<Contact>() on a.Id equals c.ParentCustomerId.Id
                where 
                    c.CreatedOn > new DateTime(2010, 11, 12) || 
                    (c.CreatedOn < new DateTime(2010, 11, 12) && c.CreatedOn == new DateTime(2010, 11, 12)) ||
                    a.Name == "First Person"
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(false),
                Criteria = new FilterExpression(LogicalOperator.Or)
                {
                    Conditions =
                    {
                        new ConditionExpression("name", ConditionOperator.Equal, "First Person"),
                        new ConditionExpression("createdon", ConditionOperator.GreaterThan, new DateTime(2010, 11, 12)) { EntityName = "c" }
                    },
                    Filters =
                    {
                        new FilterExpression(LogicalOperator.And)
                        {
                            Conditions =
                            {
                                new ConditionExpression("createdon", ConditionOperator.LessThan, new DateTime(2010, 11, 12)) { EntityName = "c"},
                                new ConditionExpression("createdon", ConditionOperator.Equal, new DateTime(2010, 11, 12)) { EntityName = "c"}
                            }
                        }
                    }
                },
                LinkEntities =
                {
                    new LinkEntity("account", "contact", "accountid", "parentcustomerid", JoinOperator.Inner)
                    {
                        EntityAlias = "c",
                        Columns = new ColumnSet(true)
                    }
                }
            });
        }
    }
}

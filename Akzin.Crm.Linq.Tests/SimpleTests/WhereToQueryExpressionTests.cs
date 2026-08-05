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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Akzin.Crm.Linq.Tests.SimpleTests
{
    [TestClass]
    public class WhereToQueryExpressionTests
    {
        [TestMethod]
        public void CallsOrganizationService()
        {
            var crm = new CrmMock();

            crm.Query<Contact>().ToList();

            crm.VerifyCalledOnce();
        }

        [TestMethod]
        public void GetAllContacts()
        {
            var crm = new CrmMock();

            crm.Query<Contact>().ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true)
            });
        }

        [TestMethod]
        public void QueryFirstPersonOnlyByFullNameUsingGetAttributeValue()
        {
            var crm = new CrmMock();

            crm.Query<Contact>().Where(x => x.GetAttributeValue<string>("fullname") == "First Person").ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("fullname", ConditionOperator.Equal, "First Person") }
                }
            });
        }


        [TestMethod]
        public void QueryModifiedByUsingGetAttributeValue()
        {
            var crm = new CrmMock();

            crm.Query<Contact>().Where(x => x.GetAttributeValue<EntityReference>("modifiedby").Id == new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ee")).ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("modifiedby", ConditionOperator.Equal, new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ee")) }
                }
            });
        }

        [TestMethod]
        public void QueryFirstPersonOnlyByFullName()
        {
            var crm = new CrmMock();

            crm.Query<Contact>().Where(x => x.Fullname == "First Person").ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("fullname", ConditionOperator.Equal, "First Person")}
                }
            });
        }

        [TestMethod]
        public void QueryFirstPersonOnlyByFirstName()
        {
            var crm = new CrmMock();

            crm.Query<Contact>().Where(x => x.Firstname == "First").ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions = { new ConditionExpression("firstname", ConditionOperator.Equal, "First") }
                }
            });
        }

        [TestMethod]
        public void QueryFirstPersonOnlyByFirstNameAndLastname()
        {
            var crm = new CrmMock();

            crm.Query<Contact>().Where(x => x.Firstname == "First" && x.Lastname == "Person").ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("firstname", ConditionOperator.Equal, "First"),
                        new ConditionExpression("lastname", ConditionOperator.Equal, "Person")
                    }
                }
            });
        }

        [TestMethod]
        public void QueryFirstPersonOnlyByFirstNameOrLastname()
        {
            var crm = new CrmMock();

            crm.Query<Contact>().Where(x => x.Firstname == "First" || x.Lastname == "Person").ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.Or)
                {
                    Conditions =
                    {
                        new ConditionExpression("firstname", ConditionOperator.Equal, "First"),
                        new ConditionExpression("lastname", ConditionOperator.Equal, "Person")
                    }
                }
            });
        }


        [TestMethod]
        public void StringEqual()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.Firstname == "First"
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("firstname", ConditionOperator.Equal, "First")
                    }
                }
            });
        }

        [TestMethod]
        public void BooleanEqual()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.GetAttributeValue<bool>("isBool") == false
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("isBool", ConditionOperator.Equal, false)
                    }
                }
            });
        }

        [TestMethod]
        public void BooleanEqualUnary()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.GetAttributeValue<bool>("isBool")
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("isBool", ConditionOperator.Equal, true)
                    }
                }
            });
        }

        [TestMethod]
        public void StringNotEqual()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.Firstname != "First"
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("firstname", ConditionOperator.NotEqual, "First")
                    }
                }
            });
        }

        [TestMethod]
        public void StringInList()
        {
            var crm = new CrmMock();

            var l = new List<string>{"First", "Second"};

            var q = from c in crm.Query<Contact>()
                where l.Contains(c.Firstname)
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("firstname", ConditionOperator.In, "First", "Second")
                    }
                }
            });
        }

        [TestMethod]
        public void StringNotInList()
        {
            var crm = new CrmMock();

            var l = new List<string> { "First", "Second" };

            var q = from c in crm.Query<Contact>()
                where !l.Contains(c.Firstname)
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("firstname", ConditionOperator.NotIn, "First", "Second")
                    }
                }
            });
        }


        [TestMethod]
        public void StringNull()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.Firstname == null
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("firstname", ConditionOperator.Null)
                    }
                }
            });
        }

        [TestMethod]
        public void StringNotNull()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.Firstname != null
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("firstname", ConditionOperator.NotNull)
                    }
                }
            });
        }

        [TestMethod]
        public void EqualsEntityReferenceId()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.ModifiedBy.Id == new Guid("1e780736-8a85-e611-80e2-005056ae6973")
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("modifiedby", ConditionOperator.Equal, new Guid("1e780736-8a85-e611-80e2-005056ae6973"))
                    }
                }
            });
        }
    }
}

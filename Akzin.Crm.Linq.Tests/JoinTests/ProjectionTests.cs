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

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Akzin.Crm.Linq.Tests.JoinTests
{
    [TestClass]
    public class ProjectionTests
    {
        [TestMethod]
        public void JoinTwoEntities()
        {
            var crm = new CrmMock(data);

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.Id
                select new { a = acc, c };

            var contactsAndAccounts = q.ToList();

            Assert.AreEqual(3, contactsAndAccounts.Count);

            Assert.AreEqual(new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ed"), contactsAndAccounts[0].c.Id);
            Assert.AreEqual(new Guid("e7e14f5a-934c-4f41-bd3c-8e7463f399c9"), contactsAndAccounts[1].c.Id);
            Assert.AreEqual(new Guid("b35a937d-57ac-4409-a2b9-e3d478264468"), contactsAndAccounts[2].c.Id);
            Assert.AreEqual("First Person", contactsAndAccounts[0].c.Fullname);
            Assert.AreEqual("Second Human", contactsAndAccounts[1].c.Fullname);
            Assert.AreEqual("Third Android", contactsAndAccounts[2].c.Fullname);


            Assert.AreEqual(new Guid("29acdb00-ffa8-4988-9a04-b2a9f1ce4586"), contactsAndAccounts[0].a.Id);
            Assert.AreEqual(new Guid("db879bda-b959-49ef-8793-c8a0cb04dd6b"), contactsAndAccounts[1].a.Id);
            Assert.AreEqual("First Company", contactsAndAccounts[0].a.Name);
            Assert.AreEqual("First Company", contactsAndAccounts[1].a.Name);
            Assert.IsNull(contactsAndAccounts[2].a);
        }

        [TestMethod]
        public void JoinTwoEntitiesReturnFirst()
        {
            var crm = new CrmMock(data);

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                select c;

            var accounts = q.ToList();

            Assert.AreEqual(3, accounts.Count);
            Assert.AreEqual(new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ed"), accounts[0].Id);
            Assert.AreEqual(new Guid("e7e14f5a-934c-4f41-bd3c-8e7463f399c9"), accounts[1].Id);
            Assert.AreEqual(new Guid("b35a937d-57ac-4409-a2b9-e3d478264468"), accounts[2].Id);
            Assert.AreEqual("First Person",  accounts[0].Fullname);
            Assert.AreEqual("Second Human",  accounts[1].Fullname);
            Assert.AreEqual("Third Android", accounts[2].Fullname);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException), "RowVersion is only available on the main from clause")]
        public void JoinTwoEntitiesReturnSecondRowVersion()
        {
            var crm = new CrmMock(data);

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                select acc.RowVersion;

            q.ToList();
        }

        [TestMethod]
        public void JoinTwoEntitiesReturnSecond()
        {
            var crm = new CrmMock(data);

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                select acc;

            var accounts = q.ToList();

            Assert.AreEqual(3, accounts.Count);
            Assert.AreEqual(new Guid("29acdb00-ffa8-4988-9a04-b2a9f1ce4586"), accounts[0].Id);
            Assert.AreEqual(new Guid("db879bda-b959-49ef-8793-c8a0cb04dd6b"), accounts[1].Id);
            Assert.AreEqual("First Company", accounts[0].Name);
            Assert.AreEqual("First Company", accounts[1].Name);
        }

        [TestMethod]
        public void JoinTwoEntitiesReturnSecondName()
        {
            var crm = new CrmMock(data);

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                select acc.Name;

            var accountNames = q.ToList();

            Assert.AreEqual(3, accountNames.Count);
            Assert.AreEqual("First Company", accountNames[0]);
            Assert.AreEqual("First Company", accountNames[1]);
            Assert.AreEqual(null, accountNames[2]);
        }

        [TestMethod]
        public void JoinTwoEntitiesReturnFullNameAndSecondName()
        {
            var crm = new CrmMock(data);

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                select new {c.Fullname, acc.Name};

            var contactsAndAccounts = q.ToList();

            Assert.AreEqual(3, contactsAndAccounts.Count);
            Assert.AreEqual("First Person", contactsAndAccounts[0].Fullname);
            Assert.AreEqual("Second Human", contactsAndAccounts[1].Fullname);
            Assert.AreEqual("Third Android", contactsAndAccounts[2].Fullname);
            Assert.AreEqual("First Company", contactsAndAccounts[0].Name);
            Assert.AreEqual("First Company", contactsAndAccounts[1].Name);
            Assert.AreEqual(null, contactsAndAccounts[2].Name);
        }

        [TestMethod]
        public void JoinTwoEntitiesReturnFullNameAndSecondNameUsingGetAttributeValue()
        {
            var crm = new CrmMock(data);

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                select new { c.Fullname, Name = acc.GetAttributeValue<string>("name") };

            var contactsAndAccounts = q.ToList();

            Assert.AreEqual(3, contactsAndAccounts.Count);
            Assert.AreEqual("First Person", contactsAndAccounts[0].Fullname);
            Assert.AreEqual("Second Human", contactsAndAccounts[1].Fullname);
            Assert.AreEqual("Third Android", contactsAndAccounts[2].Fullname);
            Assert.AreEqual("First Company", contactsAndAccounts[0].Name);
            Assert.AreEqual("First Company", contactsAndAccounts[1].Name);
            Assert.AreEqual(null, contactsAndAccounts[2].Name);
        }


        private readonly List<Entity> data = new List<Entity>
        {
            new Contact
            {
                Id = new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ed"),
                Attributes = new AttributeCollection
                {
                    {"contactid", new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ed") },
                    {"firstname", "First"},
                    {"lastname", "Person"},
                    {"fullname", "First Person"},
                    {"acc.accountid", new AliasedValue("account", "accountid", new Guid("29acdb00-ffa8-4988-9a04-b2a9f1ce4586")) },
                    {"acc.name", new AliasedValue("account", "name", "First Company") },
                    {"acc.createdon", new AliasedValue("account", "createdon", new DateTime(2010, 11, 12)) }
                }
            },
            new Contact
            {
                Id = new Guid("e7e14f5a-934c-4f41-bd3c-8e7463f399c9"),
                Attributes = new AttributeCollection
                {
                    {"contactid", new Guid("e7e14f5a-934c-4f41-bd3c-8e7463f399c9") },
                    {"firstname", "Second"},
                    {"lastname", "Human"},
                    {"fullname", "Second Human"},
                    {"acc.accountid", new AliasedValue("account", "accountid", new Guid("db879bda-b959-49ef-8793-c8a0cb04dd6b")) },
                    {"acc.name", new AliasedValue("account", "name", "First Company") },
                    {"acc.createdon", new AliasedValue("account", "createdon", new DateTime(2010, 11, 12)) }
                }
            },
            new Contact
            {
                Id = new Guid("b35a937d-57ac-4409-a2b9-e3d478264468"),
                Attributes = new AttributeCollection
                {
                    {"contactid", new Guid("b35a937d-57ac-4409-a2b9-e3d478264468") },
                    {"firstname", "Third"},
                    {"lastname", "Android"},
                    {"fullname", "Third Android"}
                }
            }
        };
    }
}
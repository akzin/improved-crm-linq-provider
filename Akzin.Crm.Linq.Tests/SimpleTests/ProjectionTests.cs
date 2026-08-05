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
    public class ProjectionTests
    {
        [TestMethod]
        public void GetProjectedEmptyValues()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => x.ParentCustomerId).ToList();

            Assert.AreEqual(2, contacts.Count);
            Assert.AreEqual(null, contacts[0]);
            Assert.AreEqual(null, contacts[1]);
        }

        [TestMethod]
        public void GetFullEntities()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().ToList();

            Assert.AreEqual("First Person" , contacts[0].Fullname);
            Assert.AreEqual("Second Human", contacts[1].Fullname);
            Assert.AreEqual(new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ed"), contacts[0].Id);
            Assert.AreEqual(new Guid("e7e14f5a-934c-4f41-bd3c-8e7463f399c9"), contacts[1].Id);
            Assert.AreEqual("Record#1", contacts[0].RowVersion);
            Assert.AreEqual("Record#2", contacts[1].RowVersion);
        }

        [TestMethod]
        public void GetProjectedFirstnames()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => x.Firstname).ToList();

            Assert.AreEqual("First", contacts[0]);
            Assert.AreEqual("Second", contacts[1]);
        }

        [TestMethod]
        public void GetProjectedFirstnamesAndIdsAndRowVersions()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => new {x.Firstname, x.Id, x.RowVersion}).ToList();


            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname", "contactid")
            });
            Assert.AreEqual("First", contacts[0].Firstname);
            Assert.AreEqual("Second", contacts[1].Firstname);
            Assert.AreEqual(new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ed"), contacts[0].Id);
            Assert.AreEqual(new Guid("e7e14f5a-934c-4f41-bd3c-8e7463f399c9"), contacts[1].Id);
            Assert.AreEqual("Record#1", contacts[0].RowVersion);
            Assert.AreEqual("Record#2", contacts[1].RowVersion);
        }

        [TestMethod]
        public void GetProjectedFirstnamesConverted()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => Convert(x.Firstname)).ToList();

            Assert.AreEqual("Converted First", contacts[0]);
            Assert.AreEqual("Converted Second", contacts[1]);
        }

        

        [TestMethod]
        public void GetProjectedFirstnamesGetAttributeValue()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => x.GetAttributeValue<string>("firstname")).ToList();

            Assert.AreEqual("First", contacts[0]);
            Assert.AreEqual("Second", contacts[1]);
        }

        [TestMethod]
        public void GetProjectedModifiedByGetAttributeValue()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => x.GetAttributeValue<EntityReference>("modifiedby").Id).ToList();

            Assert.AreEqual(new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ee"), contacts[0]);
            Assert.AreEqual(new Guid("e7e14f5a-934c-4f41-bd3c-8e7463f399c0"), contacts[1]);
        }

        [TestMethod]
        public void GetProjectedFirstnamesAndLastnames()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => new { x.Firstname, x.Lastname}).ToList();

            Assert.AreEqual("First", contacts[0].Firstname);
            Assert.AreEqual("Person", contacts[0].Lastname);
            Assert.AreEqual("Second", contacts[1].Firstname);
            Assert.AreEqual("Human", contacts[1].Lastname);
        }

        [TestMethod]
        public void GetProjectedFirstnamesAndLastnamesMemberInit()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => new FullName  { First = x.Firstname, Lastname = x.Lastname }).ToList();

            Assert.AreEqual("First", contacts[0].First);
            Assert.AreEqual("Person", contacts[0].Lastname);
            Assert.AreEqual("Second", contacts[1].First);
            Assert.AreEqual("Human", contacts[1].Lastname);
        }

        [TestMethod]
        public void GetProjectedFirstnamesAndLastnamesMemberInitConverted()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => new FullName { First = Convert(x.Firstname), Lastname = Convert(x.Lastname)}).ToList();

            Assert.AreEqual("Converted First", contacts[0].First);
            Assert.AreEqual("Converted Person", contacts[0].Lastname);
            Assert.AreEqual("Converted Second", contacts[1].First);
            Assert.AreEqual("Converted Human", contacts[1].Lastname);
        }

        [TestMethod]
        public void GetProjectedFirstnamesAndLastnamesAndIds()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => new { x.Firstname, x.Lastname, x.Id }).ToList();

            Assert.AreEqual(new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ed"), contacts[0].Id);
            Assert.AreEqual("First", contacts[0].Firstname);
            Assert.AreEqual("Person", contacts[0].Lastname);
            Assert.AreEqual(new Guid("e7e14f5a-934c-4f41-bd3c-8e7463f399c9"), contacts[1].Id);
            Assert.AreEqual("Second", contacts[1].Firstname);
            Assert.AreEqual("Human", contacts[1].Lastname);
        }

        [TestMethod]
        public void GetProjectedFirstnamesAndLastnamesAliased()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => new { F = x.Firstname, L = x.Lastname }).ToList();

            Assert.AreEqual("First", contacts[0].F);
            Assert.AreEqual("Person", contacts[0].L);
            Assert.AreEqual("Second", contacts[1].F);
            Assert.AreEqual("Human", contacts[1].L);
        }

        [TestMethod]
        public void GetProjectedEnumOptionSet()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => x.StatusCode).ToList();

            Assert.AreEqual(new OptionSetValue(123), contacts[0]);
            Assert.AreEqual(new OptionSetValue(456), contacts[1]);
        }

        [TestMethod]
        public void GetProjectedEnumOptionSetValue()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => x.StatusCode.Value).ToList();

            Assert.AreEqual(123, contacts[0]);
            Assert.AreEqual(456, contacts[1]);
        }

        [TestMethod]
        public void GetProjectedEnum()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => x.StatusCodeEnum).ToList();

            Assert.AreEqual((ContactStatuscode)123, contacts[0]);
            Assert.AreEqual((ContactStatuscode)456, contacts[1]);
        }

        [TestMethod]
        public void GetProjectedAnonymousOptionSetValueAndOptionSetAndEnum()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => new  { x.StatusCode, x.StatusCode.Value, E = x.StatusCodeEnum}).ToList();

            Assert.AreEqual(123, contacts[0].Value);
            Assert.AreEqual(456, contacts[1].Value);
            Assert.AreEqual(new OptionSetValue(123), contacts[0].StatusCode);
            Assert.AreEqual(new OptionSetValue(456), contacts[1].StatusCode);
            Assert.AreEqual((ContactStatuscode) 123, contacts[0].E);
            Assert.AreEqual((ContactStatuscode) 456, contacts[1].E);
        }

        [TestMethod]
        public void GetEntityReference()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => x.ModifiedBy).ToList();

            Assert.AreEqual(new EntityReference("systemuser", new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ee")), contacts[0]);
            Assert.AreEqual(new EntityReference("systemuser", new Guid("e7e14f5a-934c-4f41-bd3c-8e7463f399c0")), contacts[1]);
        }

        [TestMethod]
        public void GetProjectedEntityReferenceAndId()
        {
            var crm = new CrmMock(data);

            var contacts = crm.Query<Contact>().Select(x => new {x.ModifiedBy, x.ModifiedBy.Id}).ToList();

            Assert.AreEqual(new EntityReference("systemuser", new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ee")), contacts[0].ModifiedBy);
            Assert.AreEqual(new EntityReference("systemuser", new Guid("e7e14f5a-934c-4f41-bd3c-8e7463f399c0")), contacts[1].ModifiedBy);
            Assert.AreEqual(new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ee"), contacts[0].Id);
            Assert.AreEqual(new Guid("e7e14f5a-934c-4f41-bd3c-8e7463f399c0"), contacts[1].Id);
        }

        private static string Convert(string text)
        {
            return "Converted " + text;
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
                    {"modifiedby", new EntityReference("systemuser", new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ee"))},
                    {"statuscode", new OptionSetValue(123)}
                },
                RowVersion = "Record#1"
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
                    {"modifiedby", new EntityReference("systemuser", new Guid("e7e14f5a-934c-4f41-bd3c-8e7463f399c0"))},
                    {"statuscode", new OptionSetValue(456)}
                },
                RowVersion = "Record#2"
            }
        };


        class FullName
        {
            public string First { get; set; }
            public string Lastname { get; set; }
        }
    }
}
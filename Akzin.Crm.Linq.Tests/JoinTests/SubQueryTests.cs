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

namespace Akzin.Crm.Linq.Tests.JoinTests
{
    [TestClass]
    public class SubQueryTests
    {
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException), "Sub-queries are not supported")]
        public void SubQuery()
        {
            var crm = new CrmMock(data);

            var q = from c in crm.Query<Contact>()
                select (from a in crm.Query<Account>()
                        where a.AccountId == c.ParentCustomerId.Id
                        select a);

            q.ToList();
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

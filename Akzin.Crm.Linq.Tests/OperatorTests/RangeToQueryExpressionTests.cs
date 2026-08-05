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
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Akzin.Crm.Linq.Tests.OperatorTests
{
    [TestClass]
    public class RangeToQueryExpressionTests
    {
        [TestMethod]
        public void First()
        {
            var crm = new CrmMock(data);

            var q = from c in crm.Query<Contact>()
                select c;
            q.First();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                TopCount = 1,
                ColumnSet = new ColumnSet(true)
            });
        }

        [TestMethod]
        public void FirstOrDefault()
        {
            var crm = new CrmMock(data);

            var q = from c in crm.Query<Contact>()
                select c;
            q.FirstOrDefault();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                TopCount = 1,
                ColumnSet = new ColumnSet(true)
            });
        }

        [TestMethod]
        public void Single()
        {
            var crm = new CrmMock(data);

            var q = from c in crm.Query<Contact>()
                select c;
            q.Single();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                TopCount = 2,
                ColumnSet = new ColumnSet(true)
            });
        }

        [TestMethod]
        public void SingleOrDefault()
        {
            var crm = new CrmMock(data);

            var q = from c in crm.Query<Contact>()
                select c;
            q.SingleOrDefault();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                TopCount = 2,
                ColumnSet = new ColumnSet(true)
            });
        }


        [TestMethod]
        public void Any()
        {
            var crm = new CrmMock(data);

            var q = from c in crm.Query<Contact>()
                select c;
            var result = q.Any();

            Assert.AreEqual(true, result);
            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                TopCount = 1,
                ColumnSet = new ColumnSet(true)
            });
        }

        [TestMethod]
        public void AnyWithFilterTrue()
        {
            var crm = new CrmMock(data);

            var q = from c in crm.Query<Contact>()
                where c.Firstname == "First"
                select c;
            var result = q.Any();

            Assert.AreEqual(true, result);
            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                TopCount = 1,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("firstname", ConditionOperator.Equal, "First")}
                }
            });
        }

        [TestMethod]
        public void AnyWithFilterFalse()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                where c.Firstname == "FirstXX"
                select c;
            var result = q.Any();

            Assert.AreEqual(false, result);
            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                TopCount = 1,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("firstname", ConditionOperator.Equal, "FirstXX") }
                }
            });
        }


        [TestMethod]
        public void Take3()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                select c;
            q.Take(3).ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                TopCount = 3,
                ColumnSet = new ColumnSet(true)
            });
        }

        [TestMethod]
        public void Skip2Take30()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                select c;
            q.Skip(2).Take(30).ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                //TopCount = 30,
                PageInfo = new PagingInfo
                {
                    Count = 2,
                    PageNumber = 2
                },
                ColumnSet = new ColumnSet(true)
            });
        }


        [TestMethod]
        public void SimpleOrderByAsc()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                orderby c.Firstname
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Orders = { new OrderExpression("firstname", OrderType.Ascending) }
            });
        }

        [TestMethod]
        public void SimpleOrderByDesc()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                orderby c.Firstname descending
                select c;
            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Orders = { new OrderExpression("firstname", OrderType.Descending) }
            });
        }

        [TestMethod]
        public void OrderByJoinedEntity()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                orderby acc.Name descending
                select c;

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                LinkEntities =
                {
                    new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.Inner)
                    {
                        EntityAlias = "acc",
                        Orders = { new OrderExpression("name", OrderType.Descending) }
                    }
                }
            });
        }

        [TestMethod]
        public void OrderByFirstNameAscAndJoinedEntityDesc()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                orderby c.Firstname, acc.Name descending
                select c;

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Orders = { new OrderExpression("firstname", OrderType.Ascending) },
                LinkEntities =
                {
                    new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.Inner)
                    {
                        EntityAlias = "acc",
                        Orders = { new OrderExpression("name", OrderType.Descending) }
                    }
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException), "No more than one take allowed")]
        public void TwoTakes()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                select c;
            q.Take(1).Take(2).ToList();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException), "No more than one skip allowed")]
        public void TwoSkips()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                select c;
            q.Skip(1).Skip(2).ToList();
        }

        [TestMethod]
        public void Distinct()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                join acc in crm.Query<Account>() on c.ParentCustomerId.Id equals acc.AccountId
                orderby c.Firstname, acc.Name descending
                select c;

            q.Distinct().ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                Distinct = true,
                ColumnSet = new ColumnSet(true),
                Orders = { new OrderExpression("firstname", OrderType.Ascending) },
                LinkEntities =
                {
                    new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.Inner)
                    {
                        EntityAlias = "acc",
                        Orders = { new OrderExpression("name", OrderType.Descending) }
                    }
                }
            });
        }

        [TestMethod]
        public void Count()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                select c;
            q.Count();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                PageInfo = new PagingInfo
                {
                  ReturnTotalRecordCount  = true,
                  PageNumber = 1,
                  Count = 1
                },
                ColumnSet = new ColumnSet(true)
            });
        }

        [TestMethod]
        public void DistinctCount()
        {
            var crm = new CrmMock();

            var q = from c in crm.Query<Contact>()
                select c;
            q.Distinct().Count();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                Distinct = true,
                PageInfo = new PagingInfo
                {
                    ReturnTotalRecordCount = true,
                    PageNumber = 1,
                    Count = 1
                },
                ColumnSet = new ColumnSet(true)
            });
        }

        [TestMethod]
        public void TakeThenCountDoesNotExceedTakeLimit()
        {
            var service = new RecordingOrganizationService
            {
                TotalRecordCount = 10
            };

            var count = service.Query<Contact>().Take(3).Count();

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void MultiplePagesStartAtOneAndAdvancePageNumber()
        {
            var service = new RecordingOrganizationService
            {
                Pages = new Queue<EntityCollection>(new[]
                {
                    Page(new Contact { Id = Guid.NewGuid() }, true, "cookie-1"),
                    Page(new Contact { Id = Guid.NewGuid() }, false, null)
                })
            };

            var contacts = service.Query<Contact>().ToList();

            Assert.AreEqual(2, contacts.Count);
            CollectionAssert.AreEqual(new[] { 1, 2 }, service.RequestedPageNumbers.ToArray());
        }

        private static EntityCollection Page(Entity entity, bool moreRecords, string pagingCookie)
        {
            return new EntityCollection(new List<Entity> { entity })
            {
                MoreRecords = moreRecords,
                PagingCookie = pagingCookie
            };
        }

        private sealed class RecordingOrganizationService : IOrganizationService
        {
            public RecordingOrganizationService()
            {
                Pages = new Queue<EntityCollection>();
            }

            public List<int> RequestedPageNumbers { get; } = new List<int>();
            public Queue<EntityCollection> Pages { get; set; }
            public int TotalRecordCount { get; set; }

            public OrganizationResponse Execute(OrganizationRequest request)
            {
                var retrieve = request as RetrieveMultipleRequest;
                if (retrieve == null)
                    throw new NotSupportedException();

                var query = (QueryExpression)retrieve.Query;
                RequestedPageNumbers.Add(query.PageInfo.PageNumber);

                var collection = Pages.Count == 0
                    ? new EntityCollection()
                    : Pages.Dequeue();
                collection.TotalRecordCount = TotalRecordCount;

                return new RetrieveMultipleResponse
                {
                    Results = { [nameof(EntityCollection)] = collection }
                };
            }

            public Guid Create(Entity entity) { throw new NotSupportedException(); }
            public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet) { throw new NotSupportedException(); }
            public void Update(Entity entity) { throw new NotSupportedException(); }
            public void Delete(string entityName, Guid id) { throw new NotSupportedException(); }
            public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) { throw new NotSupportedException(); }
            public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) { throw new NotSupportedException(); }
            public EntityCollection RetrieveMultiple(QueryBase query) { throw new NotSupportedException(); }
        }

        private readonly List<Entity> data = new List<Entity>
        {
            new Contact
            {
                Attributes = new AttributeCollection
                {
                    {"contactid", new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ed") },
                    {"firstname", "First"},
                    {"lastname", "Person"},
                    {"fullname", "First Person"},
                    {"modifiedby", new EntityReference("systemuser", new Guid("dd96fc4c-6f08-4369-a0a1-e05e7311b9ee"))},
                    {"statuscode", new OptionSetValue(123)}
                }
            }
        };
    }
}

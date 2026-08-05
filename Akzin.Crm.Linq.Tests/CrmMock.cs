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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Akzin.Crm.Linq.Tests
{
    class CrmMock : Mock<IOrganizationService>, IOrganizationService
    {
        private readonly List<Entity> retriveMultipleResult;
        private string queryExpressionJson;

        public CrmMock() : this(new List<Entity>())
        {
        }

        public CrmMock(List<Entity> retrieveMultipleResult) : base(MockBehavior.Strict)
        {
            retriveMultipleResult = retrieveMultipleResult;

            Setup(crm => crm.Execute(It.Is<RetrieveMultipleRequest>(retrieveMultipleRequest => retrieveMultipleRequest.Query is QueryExpression)))
                .Returns<RetrieveMultipleRequest>(ExecuteRetrieveMultiple);
        }

        public void VerifyCalledOnce()
        {
            Verify(x => x.Execute(It.IsAny<RetrieveMultipleRequest>()), Times.Once());
        }
        public void VerifyCalledOnceAndExpected(QueryExpression expected)
        {
            VerifyCalledOnce();
            VerifyExpected(expected);
        }

        public void VerifyExpected(QueryExpression expected)
        {
            // DataFetcher explicitly sends the first CRM page as page 1. Most query
            // translation tests don't specify paging, so normalize that implicit value.
            if (expected.PageInfo.PageNumber == 0 && expected.PageInfo.Count == 0)
            {
                expected.PageInfo.PageNumber = 1;
            }

            var actualJson = queryExpressionJson;
            var expectedJson = expected.ToJson();
            Assert.AreEqual(expectedJson, actualJson);
        }

        private RetrieveMultipleResponse ExecuteRetrieveMultiple(RetrieveMultipleRequest request)
        {
            queryExpressionJson = (request.Query as QueryExpression).ToJson();

            var response = new RetrieveMultipleResponse
            {
                Results = { [nameof(EntityCollection)] = new EntityCollection(retriveMultipleResult) }
            };
            return response;
        }

        public Guid Create(Entity entity)
        {
            return Object.Create(entity);
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            return Object.Retrieve(entityName, id, columnSet);
        }

        public void Update(Entity entity)
        {
            Object.Update(entity);
        }

        public void Delete(string entityName, Guid id)
        {
            Object.Delete(entityName, id);
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            return Object.Execute(request);
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            Object.Associate(entityName, entityId, relationship, relatedEntities);
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship,
            EntityReferenceCollection relatedEntities)
        {
            Object.Disassociate(entityName, entityId, relationship, relatedEntities);
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            return Object.RetrieveMultiple(query);
        }
    }
}

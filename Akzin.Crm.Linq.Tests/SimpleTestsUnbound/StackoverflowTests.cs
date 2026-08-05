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
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Akzin.Crm.Linq.Tests.SimpleTestsUnbound
{
    [TestClass]
    public class StackoverflowTests
    {
        //https://stackoverflow.com/questions/45675967/date-range-filter-in-linq-to-crm
        [TestMethod]
        public void TestStackoverflow1()
        {
            var crm = new CrmMock();

            var info = new
            {
                FromDate = (DateTime?)DateTime.Now.AddDays(-1),
                ToDate = (DateTime?)DateTime.Now.AddDays(1)
            };

            //using Akzin.Crm.Linq;
            IQueryable<Entity> q = crm.Query("contact");

            if (info.FromDate != null && info.ToDate != null)
            {
                q = q.Where(r => r.GetAttributeValue<DateTime>("rundate") >= info.FromDate.Value.Date && r.GetAttributeValue<DateTime>("rundate") <= info.ToDate.Value.Date);
            }

            q.ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("rundate", ConditionOperator.GreaterEqual, info.FromDate.Value.Date),
                        new ConditionExpression("rundate", ConditionOperator.LessEqual, info.ToDate.Value.Date),
                    }
                }
            });
        }

        //https://stackoverflow.com/questions/32855148/performing-two-left-outer-joins-in-a-single-linq-to-crm-query
        [TestMethod]
        public void TestStackoverflow2()
        {
            var crm = new CrmMock();

            //using Akzin.Crm.Linq;
            var q = from a in crm.Query<Account>()
                join c in crm.Query<Contact>()
                    on a.GetAttributeValue<EntityReference>("primarycontactid").Id equals c.ContactId
                    into gr
                from c_joined in gr.DefaultIfEmpty()
                join c in crm.Query<Contact>()
                    on a.Name equals c.GetAttributeValue<string>("fullname")
                    into gr2
                from c2_joined in gr2.DefaultIfEmpty()
                select new
                {
                    contact_name = c_joined.GetAttributeValue<string>("fullname"),
                    account_name = a.Name,
                    other_name = c2_joined.GetAttributeValue<string>("fullname")
                };

            q.ToList();

            //crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            //{
            //    ColumnSet = new ColumnSet(true),
            //    Criteria = new FilterExpression
            //    {
            //        //Conditions =
            //        //{
            //        //    new ConditionExpression("rundate", ConditionOperator.GreaterEqual, info.FromDate.Value.Date),
            //        //    new ConditionExpression("rundate", ConditionOperator.LessEqual, info.ToDate.Value.Date),
            //        //}
            //    }
            //});
        }

        ////https://stackoverflow.com/questions/7212070/linq-error-when-using-defaultifempty?rq=1
        //[TestMethod]
        //public void TestStackoverflow3()
        //{
        //    var crm = new CrmMock();

        //    //using Akzin.Crm.Linq;
        //    var q = (from r in crm.Query("opportunity")
        //        join a in crm.Query("account") on ((EntityReference)r["accountid"]).Id equals a["accountid"]
        //        join c in crm.Query("contact") on ((EntityReference)r["new_contact"]).Id equals c["contactid"]
        //        where ((EntityReference)r["new_channelpartner"]).Id.Equals(new Guid("c55c2e09-a3be-e011-8b2e-00505691002b"))
        //        select new
        //        {
        //            OpportunityId = !r.Contains("opportunityid") ? string.Empty : r["opportunityid"],
        //            CustomerId = !r.Contains("customerid") ? string.Empty : ((EntityReference)r["customerid"]).Name,
        //            Priority = !r.Contains("opportunityratingcode") ? string.Empty : r.FormattedValues["opportunityratingcode"],
        //            ContactName = !r.Contains("new_contact") ? string.Empty : ((EntityReference)r["new_contact"]).Name,
        //            Source = !r.Contains("new_source") ? string.Empty : r["new_source"],
        //            CreatedOn = !r.Contains("createdon") ? string.Empty : r["createdon"],
        //            State = !a.Contains("address1_stateorprovince") ? string.Empty : a["address1_stateorprovince"],
        //            Zip = !a.Contains("address1_postalcode") ? string.Empty : a["address1_postalcode"],
        //            Eval = !r.Contains("new_colderevaluation") ? string.Empty : r.FormattedValues["new_colderevaluation"],
        //            DistributorName = !r.Contains("new_channelpartner") ? string.Empty : ((EntityReference)r["new_channelpartner"]).Name,
        //            ContactStreetAddress = !c.Contains("address1_line1") ? string.Empty : c["address1_line1"]
        //        });

        //    q.ToList();

        //    crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
        //    {
        //        ColumnSet = new ColumnSet(true),
        //        Criteria = new FilterExpression
        //        {
        //            //Conditions =
        //            //{
        //            //    new ConditionExpression("rundate", ConditionOperator.GreaterEqual, info.FromDate.Value.Date),
        //            //    new ConditionExpression("rundate", ConditionOperator.LessEqual, info.ToDate.Value.Date),
        //            //}
        //        }
        //    });
        //}
    }
}

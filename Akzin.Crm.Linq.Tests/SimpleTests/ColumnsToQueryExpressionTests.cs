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

namespace Akzin.Crm.Linq.Tests.SimpleTests
{
    [TestClass]
    public class ColumnsToQueryExpressionTests
    {
        [TestMethod]
        public void QueryAllColumns()
        {
            var crm = new CrmMock();

            crm.Query<Contact>().ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true)
            });
        }

        [TestMethod]
        public void QueryFirstnameOnly()
        {
            var crm = new CrmMock();

            crm.Query<Contact>().Select(x => x.Firstname).ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname")
            });
        }

        [TestMethod]
        public void QueryFirstnameAndLastname()
        {
            var crm = new CrmMock();

            crm.Query<Contact>().Select(x => new {x.Firstname, x.Lastname}).ToList();

            crm.VerifyCalledOnceAndExpected(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname", "lastname")
            });
        }
    }
}

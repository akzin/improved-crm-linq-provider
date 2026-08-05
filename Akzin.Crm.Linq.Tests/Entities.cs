using System;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

[assembly: ProxyTypesAssembly]

namespace Akzin.Crm.Linq.Tests
{
    [EntityLogicalName("contact")]
    public class Contact : Entity
    {
        [AttributeLogicalName("contactid")]
        public override Guid Id
        {
            get => base.Id;
            set => ContactId = value;
        }

        [AttributeLogicalName("contactid")]
        public Guid? ContactId
        {
            get => GetAttributeValue<Guid?>("contactid");
            set
            {
                SetAttributeValue("contactid", value);
                base.Id = value ?? Guid.Empty;
            }
        }

        [AttributeLogicalName("firstname")]
        public string Firstname
        {
            get => GetAttributeValue<string>("firstname");
            set => SetAttributeValue("firstname", value);
        }

        [AttributeLogicalName("lastname")]
        public string Lastname
        {
            get => GetAttributeValue<string>("lastname");
            set => SetAttributeValue("lastname", value);
        }

        [AttributeLogicalName("fullname")]
        public string Fullname
        {
            get => GetAttributeValue<string>("fullname");
            set => SetAttributeValue("fullname", value);
        }

        [AttributeLogicalName("gendercode")]
        public string GenderCode
        {
            get => GetAttributeValue<string>("gendercode");
            set => SetAttributeValue("gendercode", value);
        }

        [AttributeLogicalName("createdon")]
        public DateTime? CreatedOn
        {
            get => GetAttributeValue<DateTime?>("createdon");
            set => SetAttributeValue("createdon", value);
        }

        [AttributeLogicalName("parentcustomerid")]
        public EntityReference ParentCustomerId
        {
            get => GetAttributeValue<EntityReference>("parentcustomerid");
            set => SetAttributeValue("parentcustomerid", value);
        }

        [AttributeLogicalName("statuscode")]
        public OptionSetValue StatusCode
        {
            get => GetAttributeValue<OptionSetValue>("statuscode");
            set => SetAttributeValue("statuscode", value);
        }

        [AttributeLogicalName("statuscode")]
        public ContactStatuscode? StatusCodeEnum
        {
            get => (ContactStatuscode?)StatusCode?.Value;
            set => StatusCode = value.HasValue ? new OptionSetValue((int)value) : null;
        }

        [AttributeLogicalName("modifiedby")]
        public EntityReference ModifiedBy => GetAttributeValue<EntityReference>("modifiedby");


        [AttributeLogicalName("datestart")]
        public DateTime? DateStart
        {
            get => GetAttributeValue<DateTime?>("datestart");
            set => SetAttributeValue("datestart", value);
        }

        [AttributeLogicalName("dateend")]
        public DateTime? DateEnd
        {
            get => GetAttributeValue<DateTime?>("dateend");
            set => SetAttributeValue("dateend", value);
        }
    }

    public enum ContactStatuscode
    {
        [EnumMember]
        Active = 1,

        [EnumMember]
        Inactive = 2,
    }

    [EntityLogicalName("account")]
    public class Account : Entity
    {
        [AttributeLogicalName("accountid")]
        public override Guid Id
        {
            get => base.Id;
            set => AccountId = value;
        }

        [AttributeLogicalName("accountid")]
        public Guid? AccountId
        {
            get => GetAttributeValue<Guid?>("accountid");
            set
            {
                SetAttributeValue("accountid", value);
                base.Id = value ?? Guid.Empty;
            }
        }

        [AttributeLogicalName("merged")]
        public bool Merged
        {
            get => GetAttributeValue<bool>("merged");
            set => SetAttributeValue("merged", value);
        }

        [AttributeLogicalName("name")]
        public string Name
        {
            get => GetAttributeValue<string>("name");
            set => SetAttributeValue("name", value);
        }

        [AttributeLogicalName("createdon")]
        public DateTime CreatedOn
        {
            get => GetAttributeValue<DateTime>("createdon");
            set => SetAttributeValue("createdon", value);
        }

        [AttributeLogicalName("preferredsystemuserid")]
        public EntityReference PreferredSystemUserId
        {
            get => GetAttributeValue<EntityReference>("preferredsystemuserid");
            set => SetAttributeValue("preferredsystemuserid", value);
        }

        [AttributeLogicalName("statuscode")]
        public// Copyright (c) Akzin.com, to@fik.email
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
            AccountStatuscode? StatusCodeEnum
        {
            get => (AccountStatuscode?)StatusCode?.Value;
            set => StatusCode = value.HasValue ? new OptionSetValue((int)value) : null;
        }

        [AttributeLogicalName("statuscode")]
        public OptionSetValue StatusCode
        {
            get => GetAttributeValue<OptionSetValue>("statuscode");
            set => SetAttributeValue("statuscode", value);
        }

        [AttributeLogicalName("modifiedby")]
        public EntityReference ModifiedBy => GetAttributeValue<EntityReference>("modifiedby");
    }

    [DataContract]
    public enum AccountStatuscode
    {
        [EnumMember]
        Active = 1,

        [EnumMember]
        Inactive = 2,
    }

    [EntityLogicalName("systemuser")]
    public class SystemUser : Entity
    {
        [AttributeLogicalName("systemuserid")]
        public override Guid Id
        {
            get => base.Id;
            set => SystemUserId = value;
        }

        [AttributeLogicalName("systemuserid")]
        public Guid? SystemUserId
        {
            get => GetAttributeValue<Guid?>("systemuserid");
            set
            {
                SetAttributeValue("systemuserid", value);
                base.Id = value ?? Guid.Empty;
            }
        }

        [AttributeLogicalName("fullname")]
        public string FullName => GetAttributeValue<string>("fullname");

        [AttributeLogicalName("firstname")]
        public string Firstname
        {
            get => GetAttributeValue<string>("firstname");
            set => SetAttributeValue("firstname", value);
        }

        [AttributeLogicalName("lastname")]
        public string Lastname
        {
            get => GetAttributeValue<string>("lastname");
            set => SetAttributeValue("lastname", value);
        }

        [AttributeLogicalName("modifiedby")]
        public EntityReference ModifiedBy => GetAttributeValue<EntityReference>("modifiedby");

    }

}

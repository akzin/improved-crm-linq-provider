using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Metadata;

namespace Akzin.Crm.Linq.Tests
{
    [TestClass]
    public class LiveTests
    {
        //[TestMethod]
        //public void LiveTest()
        //{
        //    var organizationServiceProxy = new OrganizationServiceProxy(
        //        new Uri("http://win-139liosu9nd/DataTest1/XRMServices/2011/Organization.svc"), null, null, null);
        //    organizationServiceProxy.EnableProxyTypes();

        //    var crm = new OrganizationServiceContext(organizationServiceProxy);

        //    var q = organizationServiceProxy.Query<Contact>().Select(x => x.Id).OrderBy(x => x);

        //    //var lall = q.ToList();
        //    //var l0 = q.Take(30).ToList();
        //    //var l6 = q.Skip(1).Take(30).ToList();
        //    //var l7 = q.Skip(30).Take(30).ToList();

        //    //1296
        //    var msEigenaren = (from s in crm.CreateQuery("systemuser")
        //                       join co in crm.CreateQuery("contact") on s.GetAttributeValue<Guid?>("systemuserid") equals co
        //                           .GetAttributeValue<EntityReference>("ownerid").Id
        //                       select s).ToList();

        //    var eigenaren = (from s in organizationServiceProxy.Query("systemuser")
        //                     join co in organizationServiceProxy.Query("contact") on s.GetAttributeValue<Guid?>("systemuserid") equals co
        //            .GetAttributeValue<EntityReference>("ownerid").Id
        //                     select s).ToList();

        //    var listMeneer = organizationServiceProxy.Query<Contact>().Where(x => x.Firstname == "Meneer").ToList();
        //    //var c = organizationServiceProxy.Query<Contact>().Where(x => x.Firstname == "Meneer").Count();
        //    var c = organizationServiceProxy.Query<Contact>().Distinct().Count();
        //    //var l = q.Take(5).ToList();
        //}

        //[TestMethod]
        public void LiveTest()
        {
            var organizationServiceProxy = new OrganizationServiceProxy(
                new Uri("http://win-139liosu9nd/DataTest1/XRMServices/2011/Organization.svc"), null, null, null);
            organizationServiceProxy.EnableProxyTypes();

            var crm = new OrganizationServiceContext(organizationServiceProxy);

            var spir_autonummerId = organizationServiceProxy.Query<spir_autonummergroup>().First().spir_autonummerid.Id;
            var groupReference = organizationServiceProxy.Query<spir_autonummergroup>().First().spir_groupedentityid;

            //var z = organizationServiceProxy.Query<spir_autonummer>()

            //    .Select(a => new spir_autonummer { Id = a.Id, spir_laatstuitgegeven = a.spir_laatstuitgegeven, RowVersion = a.RowVersion })

            //    .First(a => a.Id == spir_autonummerId);

            var z = organizationServiceProxy.Query<spir_autonummergroup>()
                .Where(ag => ag.spir_autonummerid.Id == spir_autonummerId && ag.spir_groupedentityid == groupReference)
                .Select(ag => new spir_autonummergroup { Id = ag.Id, spir_laatstuitgegeven = ag.spir_laatstuitgegeven, RowVersion = ag.RowVersion })
                .FirstOrDefault();
        }

        [DataContract]
        [EntityLogicalName("spir_autonummer")]
        public partial class spir_autonummer : Entity
        {
            public const string EntityLogicalName = "spir_autonummer";
            public const string EntityDisplayName = "Autonummer";

            public spir_autonummer() : base(EntityLogicalName)
            {
                Initialize();
            }

            partial void Initialize();

            [AttributeLogicalName("spir_autonummerid")]
            public override Guid Id
            {
                get { return base.Id; }
                set
                {
                    this.spir_autonummerId = value;
                    if (value == Guid.Empty) this.Attributes.Remove("spir_autonummerid");
                }
            }

            /// <summary>
            /// De unieke id van de gebruiker die de record heeft gemaakt.
            /// </summary>
            [AttributeLogicalName("createdby")]
            public EntityReference CreatedBy
            {
                get { return this.GetAttributeValue<EntityReference>("createdby"); }
            }

            /// <summary>
            /// De datum en het tijdstip waarop de record is gemaakt.
            /// </summary>
            [AttributeLogicalName("createdon")]
            public DateTime? CreatedOn
            {
                get { return this.GetAttributeValue<DateTime?>("createdon"); }
            }

            /// <summary>
            /// De unieke id van de gebruiker die de record heeft gewijzigd.
            /// </summary>
            [AttributeLogicalName("modifiedby")]
            public EntityReference ModifiedBy
            {
                get { return this.GetAttributeValue<EntityReference>("modifiedby"); }
            }

            /// <summary>
            /// De datum en het tijdstip waarop de record is gewijzigd.
            /// </summary>
            [AttributeLogicalName("modifiedon")]
            public DateTime? ModifiedOn
            {
                get { return this.GetAttributeValue<DateTime?>("modifiedon"); }
            }

            /// <summary>
            /// Eigenaar-id
            /// </summary>
            [AttributeLogicalName("ownerid")]
            public EntityReference OwnerId
            {
                get { return this.GetAttributeValue<EntityReference>("ownerid"); }
                set
                {
                    this.SetAttributeValue("ownerid", value);
                }
            }

            /// <summary>
            /// De unieke id voor de business unit die eigenaar is van de record
            /// </summary>
            [AttributeLogicalName("owningbusinessunit")]
            public EntityReference OwningBusinessUnit
            {
                get { return this.GetAttributeValue<EntityReference>("owningbusinessunit"); }
            }

            /// <summary>
            /// De unieke id voor het team dat eigenaar is van de record.
            /// </summary>
            [AttributeLogicalName("owningteam")]
            public EntityReference OwningTeam
            {
                get { return this.GetAttributeValue<EntityReference>("owningteam"); }
            }

            /// <summary>
            /// De unieke id voor de gebruiker die eigenaar is van de record.
            /// </summary>
            [AttributeLogicalName("owninguser")]
            public EntityReference OwningUser
            {
                get { return this.GetAttributeValue<EntityReference>("owninguser"); }
            }

            /// <summary>
            /// Vormt samen met de Entiteitnaam de unieke sleutel voor een autonummer-serie.Vaak wordt de naam van het attribuut gebruikt waarin het nummer wordt gezet.
            /// </summary>
            [AttributeLogicalName("spir_attribute")]
            public string spir_attribute
            {
                get { return this.GetAttributeValue<string>("spir_attribute"); }
                set
                {
                    this.SetAttributeValue("spir_attribute", value);
                }
            }

            /// <summary>
            /// De unieke id voor entiteitsexemplaren
            /// </summary>
            [AttributeLogicalName("spir_autonummerid")]
            public Guid? spir_autonummerId
            {
                get { return this.GetAttributeValue<Guid?>("spir_autonummerid"); }
                set
                {
                    this.SetAttributeValue("spir_autonummerid", value);
                    if (value.HasValue)
                    {
                        base.Id = value.Value;
                    }
                    else
                    {
                        base.Id = Guid.Empty;
                    }
                }
            }

            /// <summary>
            /// Dit is 'Sleutel 1' die samen met Sleutel 2 en evt (in geval van een  Scoped-Autonummer) de Groeperende Template de unieke sleutel bepaald voor een autonummer-serie.
            /// </summary>
            [AttributeLogicalName("spir_entityname")]
            public string spir_entityname
            {
                get { return this.GetAttributeValue<string>("spir_entityname"); }
                set
                {
                    this.SetAttributeValue("spir_entityname", value);
                }
            }

            [AttributeLogicalName("spir_groupentitypath")]
            public string spir_groupEntityPath
            {
                get { return this.GetAttributeValue<string>("spir_groupentitypath"); }
                set
                {
                    this.SetAttributeValue("spir_groupentitypath", value);
                }
            }

            /// <summary>
            /// Laatst uitgegeven volgnummer
            /// </summary>
            [AttributeLogicalName("spir_laatstuitgegeven")]
            public int? spir_laatstuitgegeven
            {
                get { return this.GetAttributeValue<int?>("spir_laatstuitgegeven"); }
                set
                {
                    this.SetAttributeValue("spir_laatstuitgegeven", value);
                }
            }

            /// <summary>
            /// De naam van de aangepaste entiteit.
            /// </summary>
            [AttributeLogicalName("spir_name")]
            public string spir_name
            {
                get { return this.GetAttributeValue<string>("spir_name"); }
                set
                {
                    this.SetAttributeValue("spir_name", value);
                }
            }

            /// <summary>
            /// Status van de/het Autonummer
            /// </summary>
            [AttributeLogicalName("statecode")]
            public spir_autonummerState? statecode
            {
                get
                {
                    OptionSetValue optionSet = this.GetAttributeValue<OptionSetValue>("statecode");
                    if ((optionSet != null))
                    {
                        return ((spir_autonummerState)(Enum.ToObject(typeof(spir_autonummerState), optionSet.Value)));
                    }
                    else
                    {
                        return null;
                    }
                }
                set
                {
                    if ((value == null))
                    {
                        this.SetAttributeValue("statecode", null);
                    }
                    else
                    {
                        this.SetAttributeValue("statecode", new OptionSetValue(((int)(value))));
                    }
                }
            }

            /// <summary>
            /// De reden van de status van de Autonummer
            /// </summary>
            [AttributeLogicalName("statuscode")]
            public OptionSetValue statuscode
            {
                get { return this.GetAttributeValue<OptionSetValue>("statuscode"); }
                set
                {
                    this.SetAttributeValue("statuscode", value);
                }
            }
            
        }

        [DataContract]
        public enum spir_autonummerState
        {
            [EnumMember]
            Actief = 0,

            [EnumMember]
            Inactief = 1,
        }

        [DataContract]
        [EntityLogicalName("spir_autonummergroup")]
        public partial class spir_autonummergroup : Entity
        {
            public const string EntityLogicalName = "spir_autonummergroup";
            public const string EntityDisplayName = "AutonummerGroup";

            public spir_autonummergroup() : base(EntityLogicalName)
            {
                Initialize();
            }

            partial void Initialize();

            [AttributeLogicalName("spir_autonummergroupid")]
            public override Guid Id
            {
                get { return base.Id; }
                set
                {
                    this.spir_autonummergroupId = value;
                    if (value == Guid.Empty) this.Attributes.Remove("spir_autonummergroupid");
                }
            }

            /// <summary>
            /// De unieke id van de gebruiker die de record heeft gemaakt.
            /// </summary>
            [AttributeLogicalName("createdby")]
            public EntityReference CreatedBy
            {
                get { return this.GetAttributeValue<EntityReference>("createdby"); }
            }

            /// <summary>
            /// De datum en het tijdstip waarop de record is gemaakt.
            /// </summary>
            [AttributeLogicalName("createdon")]
            public DateTime? CreatedOn
            {
                get { return this.GetAttributeValue<DateTime?>("createdon"); }
            }

            /// <summary>
            /// De unieke id van de gebruiker die de record heeft gewijzigd.
            /// </summary>
            [AttributeLogicalName("modifiedby")]
            public EntityReference ModifiedBy
            {
                get { return this.GetAttributeValue<EntityReference>("modifiedby"); }
            }

            /// <summary>
            /// De datum en het tijdstip waarop de record is gewijzigd.
            /// </summary>
            [AttributeLogicalName("modifiedon")]
            public DateTime? ModifiedOn
            {
                get { return this.GetAttributeValue<DateTime?>("modifiedon"); }
            }

            /// <summary>
            /// Eigenaar-id
            /// </summary>
            [AttributeLogicalName("ownerid")]
            public EntityReference OwnerId
            {
                get { return this.GetAttributeValue<EntityReference>("ownerid"); }
                set
                {
                    this.SetAttributeValue("ownerid", value);
                }
            }

            /// <summary>
            /// De unieke id voor de business unit die eigenaar is van de record
            /// </summary>
            [AttributeLogicalName("owningbusinessunit")]
            public EntityReference OwningBusinessUnit
            {
                get { return this.GetAttributeValue<EntityReference>("owningbusinessunit"); }
            }

            /// <summary>
            /// De unieke id voor het team dat eigenaar is van de record.
            /// </summary>
            [AttributeLogicalName("owningteam")]
            public EntityReference OwningTeam
            {
                get { return this.GetAttributeValue<EntityReference>("owningteam"); }
            }

            /// <summary>
            /// De unieke id voor de gebruiker die eigenaar is van de record.
            /// </summary>
            [AttributeLogicalName("owninguser")]
            public EntityReference OwningUser
            {
                get { return this.GetAttributeValue<EntityReference>("owninguser"); }
            }

            /// <summary>
            /// De unieke id voor entiteitsexemplaren
            /// </summary>
            [AttributeLogicalName("spir_autonummergroupid")]
            public Guid? spir_autonummergroupId
            {
                get { return this.GetAttributeValue<Guid?>("spir_autonummergroupid"); }
                set
                {
                    this.SetAttributeValue("spir_autonummergroupid", value);
                    if (value.HasValue)
                    {
                        base.Id = value.Value;
                    }
                    else
                    {
                        base.Id = Guid.Empty;
                    }
                }
            }

            /// <summary>
            /// De unieke id voor Autonummer wordt gekoppeld aan AutonummerGroup.
            /// </summary>
            [AttributeLogicalName("spir_autonummerid")]
            public EntityReference spir_autonummerid
            {
                get { return this.GetAttributeValue<EntityReference>("spir_autonummerid"); }
                set
                {
                    this.SetAttributeValue("spir_autonummerid", value);
                }
            }

            /// <summary>
            /// Dit is de waarde van de dynamische 'sleutel 3' die samen met de Entiteitnaam en Sleutel 2 uit de autonummer-entiteit de unieke sleutel voor de autonummer-serie bepaald.
            /// </summary>
            [AttributeLogicalName("spir_groupedentityid")]
            public string spir_groupedentityid
            {
                get { return this.GetAttributeValue<string>("spir_groupedentityid"); }
                set
                {
                    this.SetAttributeValue("spir_groupedentityid", value);
                }
            }

            [AttributeLogicalName("spir_laatstuitgegeven")]
            public int? spir_laatstuitgegeven
            {
                get { return this.GetAttributeValue<int?>("spir_laatstuitgegeven"); }
                set
                {
                    this.SetAttributeValue("spir_laatstuitgegeven", value);
                }
            }
        }

        [DataContract]
        public enum spir_autonummergroupState
        {
            [EnumMember]
            Actief = 0,

            [EnumMember]
            Inactief = 1,
        }

        [DataContract]
        public enum spir_autonummergroup_statuscode
        {
            [EnumMember]
            Actief = 1,

            [EnumMember]
            Inactief = 2,
        }

    }
}

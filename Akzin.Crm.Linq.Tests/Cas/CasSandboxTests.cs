using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Akzin.Crm.Linq.Tests.Cas
{
    [TestClass]
    public class CasSandboxTests
    {
        [TestMethod]
        public void ProviderQueryRunsInsidePartialTrustAppDomain()
        {
            using (var sandbox = CasSandbox.Create())
            {
                Assert.IsTrue(sandbox.Worker.ProviderAllowsPartiallyTrustedCallers());
                Assert.AreEqual("Ada", sandbox.Worker.ExecuteProviderQuery());
            }
        }

        [TestMethod]
        public void PublicReflectionAndExpressionCompilationAreAvailable()
        {
            using (var sandbox = CasSandbox.Create())
            {
                Assert.IsTrue(sandbox.Worker.CanUsePublicReflection());
                Assert.IsTrue(sandbox.Worker.CanCreatePublicType());
                Assert.IsTrue(sandbox.Worker.CanCompileExpression());
            }
        }

        [TestMethod]
        public void DangerousPermissionsAreDeniedByPartialTrustPolicy()
        {
            using (var sandbox = CasSandbox.Create())
            {
                // Mono exposes the CAS API but does not enforce the supplied AppDomain permission set.
                // The positive tests above remain useful there; permission denials are asserted by the
                // Windows .NET Framework runtime used by Dynamics CRM.
                if (sandbox.Worker.IsFullyTrusted)
                    return;

                Assert.IsFalse(sandbox.Worker.CanAccessFiles());
                Assert.IsFalse(sandbox.Worker.CanAccessEnvironment());
                Assert.IsFalse(sandbox.Worker.CanUseUnrestrictedReflection());
                Assert.IsFalse(sandbox.Worker.CanControlAppDomain());
                Assert.IsFalse(sandbox.Worker.CanCallUnmanagedCode());
            }
        }
    }

    internal sealed class CasSandbox : IDisposable
    {
        private readonly AppDomain appDomain;

        private CasSandbox(AppDomain appDomain, CasSandboxWorker worker)
        {
            this.appDomain = appDomain;
            Worker = worker;
        }

        public CasSandboxWorker Worker { get; private set; }

        public static CasSandbox Create()
        {
            var setup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(typeof(CasSandboxTests).Assembly.Location)
            };

            var evidence = new Evidence();
            // Use the original .NET Framework API. Mono's reference assemblies expose
            // AddHostEvidence<T>, but its runtime implementation does not provide it.
#pragma warning disable 612
            evidence.AddHost(new Zone(SecurityZone.Internet));
#pragma warning restore 612
            PermissionSet permissionSet;
            try
            {
                permissionSet = SecurityManager.GetStandardSandbox(evidence);
            }
            catch (NotImplementedException)
            {
                // Mono implements neither CAS policy resolution nor enforcement. Supplying
                // an empty set still exercises the AppDomain boundary on that runtime.
                permissionSet = new PermissionSet(PermissionState.None);
            }
            var domain = AppDomain.CreateDomain("Akzin.Crm.Linq.CasTests", evidence, setup, permissionSet);

            try
            {
                var worker = (CasSandboxWorker)domain.CreateInstanceAndUnwrap(
                    typeof(CasSandboxWorker).Assembly.FullName,
                    typeof(CasSandboxWorker).FullName);
                return new CasSandbox(domain, worker);
            }
            catch
            {
                AppDomain.Unload(domain);
                throw;
            }
        }

        public void Dispose()
        {
            if (appDomain != null)
                AppDomain.Unload(appDomain);
        }
    }

    public sealed class CasSandboxWorker : MarshalByRefObject
    {
        public bool IsFullyTrusted
        {
            get { return AppDomain.CurrentDomain.IsFullyTrusted; }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public bool CanUsePublicReflection()
        {
            return typeof(string).GetProperty("Length").GetGetMethod(false).IsPublic;
        }

        public bool ProviderAllowsPartiallyTrustedCallers()
        {
            return typeof(OrganizationServiceExtensions).Assembly
                .IsDefined(typeof(AllowPartiallyTrustedCallersAttribute), false);
        }

        public bool CanCreatePublicType()
        {
            return Activator.CreateInstance(typeof(List<int>)) != null;
        }

        public bool CanCompileExpression()
        {
            Expression<Func<int>> expression = () => 40 + 2;
            return expression.Compile()() == 42;
        }

        public string ExecuteProviderQuery()
        {
            var expectedName = "Ada";
            var service = new CasOrganizationService(expectedName);

            return service.Query("contact")
                .Where(entity => entity.GetAttributeValue<string>("firstname") == expectedName)
                .Select(entity => entity.GetAttributeValue<string>("firstname"))
                .Single();
        }

        public bool CanAccessFiles()
        {
            return CanDemand(new FileIOPermission(PermissionState.Unrestricted));
        }

        public bool CanAccessEnvironment()
        {
            return CanDemand(new EnvironmentPermission(PermissionState.Unrestricted));
        }

        public bool CanUseUnrestrictedReflection()
        {
            return CanDemand(new ReflectionPermission(PermissionState.Unrestricted));
        }

        public bool CanControlAppDomain()
        {
            return CanDemand(new SecurityPermission(SecurityPermissionFlag.ControlAppDomain));
        }

        public bool CanCallUnmanagedCode()
        {
            return CanDemand(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
        }

        private static bool CanDemand(CodeAccessPermission permission)
        {
            try
            {
                permission.Demand();
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        private sealed class CasOrganizationService : IOrganizationService
        {
            private readonly string firstname;

            public CasOrganizationService(string firstname)
            {
                this.firstname = firstname;
            }

            public OrganizationResponse Execute(OrganizationRequest request)
            {
                if (!(request is RetrieveMultipleRequest))
                    throw new NotSupportedException();

                var entity = new Entity("contact")
                {
                    Id = Guid.NewGuid(),
                    Attributes = { ["firstname"] = firstname }
                };
                return new RetrieveMultipleResponse
                {
                    Results = { ["EntityCollection"] = new EntityCollection(new List<Entity> { entity }) }
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
    }
}

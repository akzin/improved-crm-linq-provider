using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Akzin.Crm.Linq")]
[assembly: AssemblyProduct("Akzin.Crm.Linq")]
[assembly: AssemblyDescription("Improved CRM Linq Provider")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Akzin.com")]
[assembly: AssemblyCopyright("Copyright ©  2018 Akzin.com (email: to@fik.email)")]
[assembly: AssemblyTrademark("The Akzin.Crm.Linq project is licensed under the Apache Software License 2.0.")]
[assembly: AssemblyCulture("")]
//[assembly: Debuggable(DebuggableAttribute.DebuggingModes.None)]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Dynamics CRM loads plug-ins in a partially trusted AppDomain. This assembly is
// strong-named, so it must explicitly allow calls from partially trusted plug-ins.
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityRules(SecurityRuleSet.Level2)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("ff1d534a-8b12-4919-8a0e-ddef15c7b11e")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.10.0")]
[assembly: AssemblyFileVersion("1.0.10.0")]

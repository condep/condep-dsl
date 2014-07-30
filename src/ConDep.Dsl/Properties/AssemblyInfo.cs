using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ConDep.Dsl.Properties;

[assembly: AssemblyTitle("ConDep.Dsl")]
[assembly: AssemblyDescription("Note: This package is for extending the ConDep DSL. If you're looking for ConDep to do deployment or infrastructure as code, please use the ConDep package. ConDep is a highly extendable Domain Specific Language for Continuous Deployment, Continuous Delivery and Infrastructure as Code on Windows.")]
[assembly: AssemblyConfiguration("")]
[assembly: ComVisible(false)]
[assembly: AssemblyCompany("ConDep")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("Copyright © ConDep 2014")]
[assembly: AssemblyVersion("2.0.0.1")]
[assembly: AssemblyFileVersion("2.0.0.1")]
[assembly: ConDepNugetVersion("2.1.0-beta")]
[assembly: CLSCompliant(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("9c96458c-b257-4105-8def-5785c229b4c6")]
[assembly: InternalsVisibleTo("ConDep.Dsl.Tests")]

namespace ConDep.Dsl.Properties
{
    public class ConDepNugetVersionAttribute : Attribute
    {
        public string Version { get; set; }

        public ConDepNugetVersionAttribute(string version)
        {
            Version = version;
        }
    }
}

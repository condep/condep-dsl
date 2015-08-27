using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;

namespace ConDep.Dsl.Config
{
    [Serializable]
    public class ConDepOptions
    {
        public ConDepOptions()
        {
            SkipHarvesting = true;
        }

        public string Application { get; set; }
        public bool DeployOnly { get; set; }
        public bool StopAfterMarkedServer { get; set; }
        public bool ContinueAfterMarkedServer { get; set; }
        public bool DeployAllApps { get; set; }
        public Assembly Assembly { get; set; }
        public LoadBalancerSuspendMethod SuspendMode { get; set; }
        public TraceLevel TraceLevel { get; set; }
        public string WebQAddress { get; set; }
        public bool BypassLB { get; set; }
        public bool InstallWebQ { get; set; }
        public string InstallWebQOnServer { get; set; }
        public string Environment { get; set; }
        public string AssemblyName { get; set; }
        public string CryptoKey { get; set; }
        public bool DryRun { get; set; }
        public bool SkipHarvesting { get; set; }

        public bool HasApplicationDefined()
        {
            return !string.IsNullOrWhiteSpace(Application);
        }

        public void ValidateMandatoryOptions()
        {
            var missingOptions = new List<string>();
            if (string.IsNullOrWhiteSpace(AssemblyName)) missingOptions.Add("AssemblyName");
            if (string.IsNullOrWhiteSpace(Environment)) missingOptions.Add("Environment");
            if (string.IsNullOrWhiteSpace(Application)) missingOptions.Add("Application");

            if (missingOptions.Any())
            {
                throw new ConDepMissingOptionsException(missingOptions);
            }
        }
    }

    public class ConDepMissingOptionsException : Exception
    {
        public ConDepMissingOptionsException(IEnumerable<string> missingOptions) 
            : base("Missing mandatory options for " + string.Join(", ", missingOptions)) { }
    }
}
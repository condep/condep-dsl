using System;   
using System.Collections.Generic;

namespace ConDep.Dsl.Config
{
    [Serializable]
    public class ConDepEnvConfig
    {
        private string[] _powerShellScriptFolders = new string[0];
        public string EnvironmentName { get; set; }

        public string[] PowerShellScriptFolders
        {
            get { return _powerShellScriptFolders; }
            set { _powerShellScriptFolders = value; }
        }

        public LoadBalancerConfig LoadBalancer { get; set; }
        public IList<ServerConfig> Servers { get; set; }
        public IList<TiersConfig> Tiers { get; set; }
        public DeploymentUserConfig DeploymentUser { get; set; }
        public dynamic OperationsConfig { get; set; }
        public bool UsingTiers { get { return Tiers != null && Tiers.Count > 0; } }
    }
}
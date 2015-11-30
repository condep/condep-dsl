using System;
using System.Collections.Generic;
using ConDep.Dsl.LoadBalancer;

namespace ConDep.Dsl.Config
{
    [Serializable]
    public class ServerConfig
    {
        private DeploymentUserConfig _deploymentUserRemote;
        private readonly ServerInfo _serverInfo = new ServerInfo();

        public string Name { get; set; }
        public bool StopServer { get; set; }
        public IList<WebSiteConfig> WebSites { get; set; }
        public DeploymentUserConfig DeploymentUser
        {
            get { return _deploymentUserRemote ?? (_deploymentUserRemote = new DeploymentUserConfig()); }
            set { _deploymentUserRemote = value; }
        }

        public string LoadBalancerFarm { get; set; }
        public ServerLoadBalancerState LoadBalancerState { get; } = new ServerLoadBalancerState();

        public PowerShellConfig PowerShell { get; set; }

        public NodeConfig Node { get; set; }

        public ServerInfo GetServerInfo()
        {
            return _serverInfo;
        }
    }

}
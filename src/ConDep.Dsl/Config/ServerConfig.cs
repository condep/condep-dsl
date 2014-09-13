using System;
using System.Collections.Generic;

namespace ConDep.Dsl.Config
{
    [Serializable]
    public class ServerConfig
    {
        private DeploymentUserConfig _deploymentUserRemote;
        private ServerInfo _serverInfo = new ServerInfo();
        private ServerLoadBalancerState _loadBalancerState = new ServerLoadBalancerState();

        public string Name { get; set; }
        public bool StopServer { get; set; }
        public IList<WebSiteConfig> WebSites { get; set; }
        public DeploymentUserConfig DeploymentUser 
        { 
            get { return _deploymentUserRemote ?? (_deploymentUserRemote = new DeploymentUserConfig()); }
            set { _deploymentUserRemote = value; }
        }

        public string LoadBalancerFarm { get; set; }
        internal ServerLoadBalancerState LoadBalancerState { get { return _loadBalancerState; } }

        public ServerInfo GetServerInfo()
        {
            return _serverInfo;
        }
    }
}
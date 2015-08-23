using System.Collections.Generic;

namespace ConDep.Dsl.Config
{
    public interface IServerConfig
    {
        string Name { get; set; }
        bool StopServer { get; set; }
        IList<WebSiteConfig> WebSites { get; set; }
        DeploymentUserConfig DeploymentUser { get; set; }
        string LoadBalancerFarm { get; set; }
        PowerShellConfig PowerShell { get; set; }
        NodeConfig Node { get; set; }
        ServerInfo GetServerInfo();
    }
}
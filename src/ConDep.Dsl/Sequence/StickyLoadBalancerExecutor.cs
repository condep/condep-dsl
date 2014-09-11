using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Sequence
{
    public class StickyLoadBalancerExecutor : LoadBalancerExecutorBase
    {
        private readonly IEnumerable<ServerConfig> _servers;
        private readonly ILoadBalance _loadBalancer;

        public StickyLoadBalancerExecutor(IEnumerable<ServerConfig> servers, ILoadBalance loadBalancer)
        {
            _servers = servers;
            _loadBalancer = loadBalancer;
        }

        public override void Execute(IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            var servers = _servers.ToList();
            ServerConfig manuelTestServer;

            if (settings.Options.StopAfterMarkedServer)
            {
                manuelTestServer = servers.SingleOrDefault(x => x.StopServer) ?? servers.First();
                ExecuteOnServer(manuelTestServer, status, settings, _loadBalancer, true, false, token);
                return;
            }

            if (settings.Options.ContinueAfterMarkedServer)
            {
                manuelTestServer = servers.SingleOrDefault(x => x.StopServer) ?? servers.First();
                _loadBalancer.BringOnline(manuelTestServer.Name, manuelTestServer.LoadBalancerFarm, status);
                servers.Remove(manuelTestServer);
            }

            servers.ForEach(server => ExecuteOnServer(server, status, settings, _loadBalancer, true, true, token));
        }
    }
}
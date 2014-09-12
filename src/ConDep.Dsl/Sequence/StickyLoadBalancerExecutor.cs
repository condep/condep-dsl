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

        public override void BringOffline(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            var servers = _servers.ToList();

            if (settings.Options.StopAfterMarkedServer)
            {
                var manuelTestServer = servers.SingleOrDefault(x => x.StopServer) ?? servers.First();
                BringOffline(manuelTestServer, status, settings, _loadBalancer, token);
                return;
            }
            if (settings.Options.ContinueAfterMarkedServer)
            {
                var manuelTestServer = servers.SingleOrDefault(x => x.StopServer) ?? servers.First();
                if (manuelTestServer.Name.Equals(server.Name))
                {
                    return;
                }
                //BringOffline(manuelTestServer, status, settings, _loadBalancer, true, token);
                //return;
            }
            BringOffline(server, status, settings, _loadBalancer, token);
        }

        public override void BringOnline(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            BringOnline(server, status, settings, _loadBalancer, token);
        }
    }
}
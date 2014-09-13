using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations.LoadBalancer;

namespace ConDep.Dsl.Sequence
{
    public class RoundRobinLoadBalancerExecutor : LoadBalancerExecutorBase
    {
        private readonly IEnumerable<ServerConfig> _servers;
        private readonly ILoadBalance _loadBalancer;
 
        public RoundRobinLoadBalancerExecutor(IEnumerable<ServerConfig> servers, ILoadBalance loadBalancer)
        {
            _servers = servers;
            _loadBalancer = loadBalancer;
        }

        public override IEnumerable<ServerConfig> GetServerExecutionOrder(IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            var servers = settings.Config.Servers;
            if (settings.Options.StopAfterMarkedServer)
            {
                return new[] { servers.SingleOrDefault(x => x.StopServer) ?? servers.First() };
            }

            if (settings.Options.ContinueAfterMarkedServer)
            {
                var markedServer = servers.SingleOrDefault(x => x.StopServer) ?? servers.First();
                if (servers.Count == 1)
                {
                    BringOnline(markedServer, status, settings, token);
                    return new List<ServerConfig>();
                }
                markedServer.PreventDeployment = true;
            }

            return servers;
        }

        private void TurnRoundRobinServersAround(IEnumerable<ServerConfig> servers, ConDepSettings settings, CancellationToken token, int roundRobinMaxOfflineServers, IReportStatus status)
        {
            var serversToBringOnline = servers.Take(roundRobinMaxOfflineServers);
            foreach (var server in serversToBringOnline)
            {
                BringOnline(server, status, settings, _loadBalancer, token);
            }
            var serversToBringOffline = servers.Skip(roundRobinMaxOfflineServers);
            foreach (var server in serversToBringOffline)
            {
                BringOffline(server, status, settings, _loadBalancer, token);
            }
        }

        public override void BringOffline(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            var servers = _servers.ToList();
            var roundRobinMaxOfflineServers = (int)Math.Ceiling(((double)servers.Count) / 2);
            var activeServerIndex = _servers.ToList().IndexOf(server);

            if (settings.Options.StopAfterMarkedServer)
            {
                var manuelTestServer = servers.SingleOrDefault(x => x.StopServer) ?? servers.First();
                BringOffline(manuelTestServer, status, settings, _loadBalancer, token);
                return;
            }

            if (activeServerIndex == roundRobinMaxOfflineServers)
            {
                TurnRoundRobinServersAround(servers, settings, token, roundRobinMaxOfflineServers, status);
            }

            if (server.PreventDeployment) return;

            if (server.LoadBalancerState == LoadBalanceState.Offline)
                return;

            if (settings.Options.ContinueAfterMarkedServer)
            {
                BringOffline(server, status, settings, _loadBalancer, token);
                server.KeepOffline = true;
                return;
            }

            if (_servers.Count() == 1)
            {
                BringOffline(server, status, settings, _loadBalancer, token);
                return;
            }

            BringOffline(server, status, settings, _loadBalancer, token);
            server.KeepOffline = true;
        }

        public override void BringOnline(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            if (server.KeepOffline)
                return;

            BringOnline(server, status, settings, _loadBalancer, token);
        }
    }
}
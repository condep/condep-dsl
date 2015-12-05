using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;

namespace ConDep.Dsl.LoadBalancer
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

        public override IEnumerable<ServerConfig> GetServerExecutionOrder(List<ServerConfig> servers, ConDepSettings settings, CancellationToken token)
        {
            if (settings.Options.StopAfterMarkedServer)
            {
                return new[] { servers.SingleOrDefault(x => x.StopServer) ?? servers.First() };
            }

            if (settings.Options.ContinueAfterMarkedServer)
            {
                var markedServer = servers.SingleOrDefault(x => x.StopServer) ?? servers.First();
                if (servers.Count == 1)
                {
                    BringOnline(markedServer, settings, token);
                    return new List<ServerConfig>();
                }
                markedServer.LoadBalancerState.PreventDeployment = true;
            }

            return servers;
        }

        private void TurnRoundRobinServersAround(List<ServerConfig> servers, ConDepSettings settings, CancellationToken token, int roundRobinMaxOfflineServers)
        {
            var serversToBringOnline = servers.Take(roundRobinMaxOfflineServers);
            foreach (var server in serversToBringOnline)
            {
                BringOnline(server, settings, _loadBalancer, token);
            }
            var serversToBringOffline = servers.Skip(roundRobinMaxOfflineServers);
            foreach (var server in serversToBringOffline)
            {
                BringOffline(server, settings, _loadBalancer, token);
            }
        }

        public override void BringOffline(ServerConfig server, ConDepSettings settings, CancellationToken token)
        {
            if (_servers == null || !_servers.Any())
            {
                Logger.Warn("No servers available to load balancer.");
                return;
            }

            var servers = _servers.ToList();
            var roundRobinMaxOfflineServers = (int)Math.Ceiling(((double)servers.Count) / 2);
            var activeServerIndex = _servers.ToList().IndexOf(server);

            if (settings.Options.StopAfterMarkedServer)
            {
                var manuelTestServer = servers.SingleOrDefault(x => x.StopServer) ?? servers.First();
                BringOffline(manuelTestServer, settings, _loadBalancer, token);
                return;
            }

            if (activeServerIndex == roundRobinMaxOfflineServers)
            {
                TurnRoundRobinServersAround(servers, settings, token, roundRobinMaxOfflineServers);
            }

            if (server.LoadBalancerState.PreventDeployment) return;

            if (server.LoadBalancerState.CurrentState == LoadBalanceState.Offline)
                return;

            if (settings.Options.ContinueAfterMarkedServer)
            {
                BringOffline(server, settings, _loadBalancer, token);
                server.LoadBalancerState.KeepOffline = true;
                return;
            }

            if (_servers.Count() == 1)
            {
                BringOffline(server, settings, _loadBalancer, token);
                return;
            }

            BringOffline(server, settings, _loadBalancer, token);
            server.LoadBalancerState.KeepOffline = true;
        }

        public override void BringOnline(ServerConfig server, ConDepSettings settings, CancellationToken token)
        {
            if (server.LoadBalancerState.KeepOffline)
                return;

            BringOnline(server, settings, _loadBalancer, token);
        }
    }
}
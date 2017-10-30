using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;

namespace ConDep.Dsl.LoadBalancer
{
    public abstract class LoadBalancerExecutorBase
    {
        public abstract void BringOffline(ServerConfig server, ConDepSettings settings, CancellationToken token);
        public abstract void BringOnline(ServerConfig server, ConDepSettings settings, CancellationToken token);

        public virtual IEnumerable<ServerConfig> GetServerExecutionOrder(List<ServerConfig> servers,
            ConDepSettings settings, CancellationToken token)
        {
            if (settings.Options.StopAfterMarkedServer)
            {
                return new[] {servers.SingleOrDefault(x => x.StopServer) ?? servers.First()};
            }

            if (settings.Options.ContinueAfterMarkedServer)
            {
                var markedServer = servers.SingleOrDefault(x => x.StopServer) ?? servers.First();
                BringOnline(markedServer, settings, token);

                return servers.Count == 1 ? new List<ServerConfig>() : servers.Except(new[] {markedServer});
            }

            return servers;
        }

        protected void BringOffline(ServerConfig server, ConDepSettings settings, ILoadBalance loadBalancer,
            CancellationToken token)
        {
            if (settings.Config.LoadBalancer == null) return;
            if (server.LoadBalancerState.CurrentState == LoadBalanceState.Offline) return;

            Logger.WithLogSection($"Taking server [{server.Name}] offline in load balancer.", () =>
            {
                loadBalancer.BringOffline(server.Name, server.LoadBalancerFarm, LoadBalancerSuspendMethod.Suspend);
                server.LoadBalancerState.CurrentState = LoadBalanceState.Offline;
            });

        }

        protected void BringOnline(ServerConfig server, ConDepSettings settings, ILoadBalance loadBalancer,
            CancellationToken token)
        {
            if (settings.Config.LoadBalancer == null) return;
            if (server.LoadBalancerState.CurrentState == LoadBalanceState.Online) return;

            Logger.WithLogSection($"Taking server [{server.Name}] online in load balancer.", () =>
            {
                loadBalancer.BringOnline(server.Name, server.LoadBalancerFarm);
                server.LoadBalancerState.CurrentState = LoadBalanceState.Online;
            });

        }

        protected LoadBalanceState GetServerState(ServerConfig server, ILoadBalance loadBalancer)
        {
            if (loadBalancer == null) throw new ArgumentException("Missing loadbalancer");
            return loadBalancer.GetServerState(server.Name, server.LoadBalancerFarm);
        }
    }
}
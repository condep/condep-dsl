using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Operations.LoadBalancer;

namespace ConDep.Dsl.Sequence
{
    public abstract class LoadBalancerExecutorBase
    {
        //public abstract void Execute(IReportStatus status, ConDepSettings settings, CancellationToken token);
        public abstract void BringOffline(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token);
        public abstract void BringOnline(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token);

        public virtual IEnumerable<ServerConfig> GetServerExecutionOrder(IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            var servers = settings.Config.Servers;
            if (settings.Options.StopAfterMarkedServer)
            {
                return new[] { servers.SingleOrDefault(x => x.StopServer) ?? servers.First() };
            }

            if (settings.Options.ContinueAfterMarkedServer)
            {
                var markedServer = servers.SingleOrDefault(x => x.StopServer) ?? servers.First();
                BringOnline(markedServer, status, settings, token);

                return servers.Count == 1 ? new List<ServerConfig>() : servers.Except(new[] { markedServer });
            }

            return servers;
        }

        protected void BringOffline(ServerConfig server, IReportStatus status, ConDepSettings settings, ILoadBalance loadBalancer, CancellationToken token)
        {
            if (server.LoadBalancerState == LoadBalanceState.Offline) return;

            Logger.WithLogSection(string.Format("Taking server [{0}] offline in load balancer.", server.Name), () =>
            {
                loadBalancer.BringOffline(server.Name, server.LoadBalancerFarm, LoadBalancerSuspendMethod.Suspend, status);
                server.LoadBalancerState = LoadBalanceState.Offline;
            });

        }
        protected void BringOnline(ServerConfig server, IReportStatus status, ConDepSettings settings, ILoadBalance loadBalancer, CancellationToken token)
        {
            if (server.LoadBalancerState == LoadBalanceState.Online) return;

            Logger.WithLogSection(string.Format("Taking server [{0}] online in load balancer.", server.Name), () =>
            {
                loadBalancer.BringOnline(server.Name, server.LoadBalancerFarm, status);
                server.LoadBalancerState = LoadBalanceState.Online;
            });

        }

        public void DryRun()
        {
            //foreach (var item in _sequence)
            //{
            //    Logger.WithLogSection(item.Name, () => { item.DryRun(); });
            //}
        }
    }
}
using System.Collections.Generic;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Sequence
{
    public abstract class LoadBalancerExecutorBase
    {
        //public abstract void Execute(IReportStatus status, ConDepSettings settings, CancellationToken token);
        public abstract void BringOffline(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token);
        public abstract void BringOnline(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token);

        protected void BringOffline(ServerConfig server, IReportStatus status, ConDepSettings settings, ILoadBalance loadBalancer, CancellationToken token)
        {
            Logger.WithLogSection(server.Name, () =>
            {
                Logger.Info(string.Format("Taking server [{0}] offline in load balancer.", server.Name));
                loadBalancer.BringOffline(server.Name, server.LoadBalancerFarm,
                                            LoadBalancerSuspendMethod.Suspend, status);
            });

        }
        protected void BringOnline(ServerConfig server, IReportStatus status, ConDepSettings settings, ILoadBalance loadBalancer, CancellationToken token)
        {
            Logger.WithLogSection(server.Name, () =>
            {
                Logger.Info(string.Format("Taking server [{0}] online in load balancer.", server.Name));
                loadBalancer.BringOnline(server.Name, server.LoadBalancerFarm, status);
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
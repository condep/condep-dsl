using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations.LoadBalancer;

namespace ConDep.Dsl.Sequence
{
    public class StickyLoadBalancerExecutor : LoadBalancerExecutorBase
    {
        private readonly ILoadBalance _loadBalancer;

        public StickyLoadBalancerExecutor(ILoadBalance loadBalancer)
        {
            _loadBalancer = loadBalancer;
        }

        public override void BringOffline(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            if (server.LoadBalancerState.CurrentState == LoadBalanceState.Offline) return;

            BringOffline(server, status, settings, _loadBalancer, token);
        }

        public override void BringOnline(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            if (server.LoadBalancerState.CurrentState == LoadBalanceState.Online) return;

            BringOnline(server, status, settings, _loadBalancer, token);
        }
    }
}
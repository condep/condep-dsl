using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.LoadBalancer
{
    public class StickyLoadBalancerExecutor : LoadBalancerExecutorBase
    {
        private readonly ILoadBalance _loadBalancer;

        public StickyLoadBalancerExecutor(ILoadBalance loadBalancer)
        {
            _loadBalancer = loadBalancer;
        }

        public override void BringOffline(ServerConfig server, ConDepSettings settings, CancellationToken token)
        {
            if (server.LoadBalancerState.CurrentState == LoadBalanceState.Offline) return;

            BringOffline(server, settings, _loadBalancer, token);
        }

        public override void BringOnline(ServerConfig server, ConDepSettings settings, CancellationToken token)
        {
            if (server.LoadBalancerState.CurrentState == LoadBalanceState.Online) return;

            BringOnline(server, settings, _loadBalancer, token);
        }
    }
}
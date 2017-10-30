using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.LoadBalancer
{
    public class OfflinePriorityLoadBalancerExecutor : StickyLoadBalancerExecutor
    {

        private readonly ILoadBalance _loadBalancer;
        public OfflinePriorityLoadBalancerExecutor(ILoadBalance loadBalancer) : base(loadBalancer)
        {
            _loadBalancer = loadBalancer;
        }

        public override IEnumerable<ServerConfig> GetServerExecutionOrder(List<ServerConfig> servers, ConDepSettings settings, CancellationToken token)
        {
            return base.GetServerExecutionOrder(servers, settings, token).OrderBy(server =>
                GetServerState(server,_loadBalancer) == LoadBalanceState.Online );
        }
    }
}
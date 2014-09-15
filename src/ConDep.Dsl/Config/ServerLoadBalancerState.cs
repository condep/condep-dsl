using ConDep.Dsl.LoadBalancer;

namespace ConDep.Dsl.Config
{
    internal class ServerLoadBalancerState
    {
        internal LoadBalanceState? CurrentState { get; set; }
        internal bool PreventDeployment { get; set; }
        internal bool KeepOffline { get; set; }
    }
}
namespace ConDep.Dsl.LoadBalancer
{
    public class ServerLoadBalancerState
    {
        public LoadBalanceState? CurrentState { get; set; }
        public bool PreventDeployment { get; set; }
        public bool KeepOffline { get; set; }
    }
}
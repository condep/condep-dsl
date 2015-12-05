using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    /// <summary>
    /// Use this interface to implement a custom load balancer
    /// </summary>
    public interface ILoadBalance
    {
        Result BringOffline(string serverName, string farm, LoadBalancerSuspendMethod suspendMethod);
        Result BringOnline(string serverName, string farm);
        LoadBalancerMode Mode { get; set; }
    }
}
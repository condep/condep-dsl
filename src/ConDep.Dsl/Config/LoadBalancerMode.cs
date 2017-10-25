using System;

namespace ConDep.Dsl.Config
{
    /// <summary>
    /// Specifies which mode ConDep will use when interacting with a Load Balancer.
    /// </summary>
    [Serializable]
    public enum LoadBalancerMode
    {
        /// <summary>
        /// Each server will be taken offline and operations executed until there 
        /// are more servers offline than online. Then all offline servers will be 
        /// taken online and all online servers taken offline. This will have the
        /// drawback of having at least half of your servers offline during execution.
        /// </summary>
        RoundRobin,

        /// <summary>
        /// Each server will be taken offline, operations executed and then taken online
        /// again immidiately. This is the recommended approach since it causes the least
        /// downtime, but will require your load balancer to handle Sticky Sessions.
        /// </summary>
        Sticky,

        /// <summary>
        /// Like Sticky, but prioritises the already offline load balancers.
        /// </summary>
        OfflinePriority
    }
}
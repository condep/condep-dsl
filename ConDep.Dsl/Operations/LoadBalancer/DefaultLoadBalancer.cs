﻿using ConDep.Dsl.Logging;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Operations.LoadBalancer
{
    public class DefaultLoadBalancer : ILoadBalance
    {
        public void BringOffline(string serverName, LoadBalancerSuspendMethod suspendMethod, IReportStatus status)
        {
            Logger.Warn("Warning: No load balancer is used. If this is not by intention, make sure you configure a provider for load balancing.");
        }

        public void BringOnline(string serverName, IReportStatus status)
        {
            Logger.Warn("Warning: No load balancer is used. If this is not by intention, make sure you configure a provider for load balancing.");
        }
    }
}
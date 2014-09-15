using System;

namespace ConDep.Dsl.LoadBalancer
{
    public class ConDepLoadBalancerException : Exception
    {
        public ConDepLoadBalancerException(string message)
            : base(message)
        {

        }
    }
}
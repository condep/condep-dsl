using System;
using System.Collections.Generic;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Tests
{
    public class MockLoadBalancer : ILoadBalance
    {
        private readonly List<Tuple<string, string>> _onlineOfflineSequence = new List<Tuple<string, string>>();

        public Result BringOffline(string serverName, string farm, LoadBalancerSuspendMethod suspendMethod)
        {
            _onlineOfflineSequence.Add(new Tuple<string,string>(serverName, "offline"));
            return Result.SuccessChanged();
        }

        public Result BringOnline(string serverName, string farm)
        {
            _onlineOfflineSequence.Add(new Tuple<string, string>(serverName, "online"));
            return Result.SuccessChanged();
        }

        public LoadBalancerMode Mode { get; set; }

        public IList<Tuple<string, string>> OnlineOfflineSequence { get { return _onlineOfflineSequence; } }
    }
}
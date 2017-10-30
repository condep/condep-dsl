using System;
using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Config;
using ConDep.Dsl.LoadBalancer;

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

        public LoadBalanceState GetServerState(string serverName, string farm)
        {
            var serverState = _onlineOfflineSequence.FindLast(p => p.Item1 == serverName);
            if (serverState == null)
                return LoadBalanceState.Online;
            var state = serverState.Item2;
            switch (state)
            {
                case "online":
                    return LoadBalanceState.Online;
                case "offline":
                    return LoadBalanceState.Offline;
            }
            throw new Exception("Last onlineOfflineSequence state of server was neither online or offline.");
        }

        public IList<Tuple<string, string>> OnlineOfflineSequence { get { return _onlineOfflineSequence; } }
    }
}
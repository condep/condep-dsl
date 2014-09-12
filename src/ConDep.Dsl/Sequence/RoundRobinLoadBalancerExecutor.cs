using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations.LoadBalancer;

namespace ConDep.Dsl.Sequence
{
    public class RoundRobinLoadBalancerExecutor : LoadBalancerExecutorBase
    {
        private readonly IEnumerable<ServerConfig> _servers;
        private readonly ILoadBalance _loadBalancer;
        private Dictionary<string, LoadBalanceState> _serverStates = new Dictionary<string, LoadBalanceState>();
        private Dictionary<string, LoadBalanceState> _serversToKeepOffline = new Dictionary<string, LoadBalanceState>();
 
        public RoundRobinLoadBalancerExecutor(IEnumerable<ServerConfig> servers, ILoadBalance loadBalancer)
        {
            _servers = servers;
            _loadBalancer = loadBalancer;
        }

        //public override void Execute(IReportStatus status, ConDepSettings settings, CancellationToken token)
        //{
        //    var servers = _servers.ToList();
        //    var roundRobinMaxOfflineServers = (int)Math.Ceiling(((double)servers.Count) / 2);
        //    ServerConfig manuelTestServer = null;

        //    if (settings.Options.StopAfterMarkedServer)
        //    {
        //        manuelTestServer = servers.SingleOrDefault(x => x.StopServer) ?? servers.First();
        //        ExecuteOnServer(manuelTestServer, status, settings, _loadBalancer, true, false, token);
        //        return;
        //    }

        //    if (settings.Options.ContinueAfterMarkedServer)
        //    {
        //        manuelTestServer = servers.SingleOrDefault(x => x.StopServer) ?? servers.First();
        //        servers.Remove(manuelTestServer);

        //        if (roundRobinMaxOfflineServers == 1)
        //        {
        //            _loadBalancer.BringOnline(manuelTestServer.Name, manuelTestServer.LoadBalancerFarm, status);
        //        }
        //    }

        //    if (servers.Count == 1)
        //    {
        //        ExecuteOnServer(servers.First(), status, settings, _loadBalancer, true, true, token);
        //        return;
        //    }

        //    for (int execCount = 0; execCount < servers.Count; execCount++)
        //    {
        //        if (execCount == roundRobinMaxOfflineServers - (manuelTestServer == null ? 0 : 1))
        //        {
        //            TurnRoundRobinServersAround(_loadBalancer, servers, roundRobinMaxOfflineServers, manuelTestServer, status);
        //        }

        //        bool bringOnline = !(roundRobinMaxOfflineServers - (manuelTestServer == null ? 0 : 1) > execCount);
        //        ExecuteOnServer(servers[execCount], status, settings, _loadBalancer, !bringOnline, bringOnline, token);
        //    }
        //}

        private void SetServerState(ServerConfig server, LoadBalanceState state)
        {
            if (_serverStates.ContainsKey(server.Name))
            {
                _serverStates[server.Name] = state;
            }
            else
            {
                _serverStates.Add(server.Name, state);
            }
        }

        private LoadBalanceState? GetServerState(ServerConfig server)
        {
            if (_serverStates.ContainsKey(server.Name))
            {
                return _serverStates[server.Name];
            }
            return null;
        }

        private void TurnRoundRobinServersAround(ILoadBalance loadBalancer, IEnumerable<ServerConfig> servers, ConDepSettings settings, CancellationToken token, int roundRobinMaxOfflineServers, ServerConfig testServer, IReportStatus status)
        {
            if (testServer != null)
            {
                BringOnline(testServer, status, settings, _loadBalancer, token);
                SetServerState(testServer, LoadBalanceState.Online);
                //loadBalancer.BringOnline(testServer.Name, testServer.LoadBalancerFarm, status);
            }
            var numberOfServers = roundRobinMaxOfflineServers - (testServer == null ? 0 : 1);

            var serversToBringOnline = servers.Take(numberOfServers);
            foreach (var server in serversToBringOnline)
            {
                BringOnline(server, status, settings, _loadBalancer, token);
                SetServerState(server, LoadBalanceState.Online);
                //loadBalancer.BringOnline(server.Name, server.LoadBalancerFarm, status);
            }
            var serversToBringOffline = servers.Skip(numberOfServers);
            foreach (var server in serversToBringOffline)
            {
                BringOffline(server, status, settings, _loadBalancer, token);
                SetServerState(server, LoadBalanceState.Offline);
                //loadBalancer.BringOffline(server.Name, server.LoadBalancerFarm, LoadBalancerSuspendMethod.Suspend, status);
            }
        }

        public override void BringOffline(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            if (GetServerState(server) == LoadBalanceState.Offline)
                return;

            var servers = _servers.ToList();
            var roundRobinMaxOfflineServers = (int)Math.Ceiling(((double)servers.Count) / 2);
            ServerConfig manuelTestServer = null;

            if (settings.Options.StopAfterMarkedServer)
            {
                manuelTestServer = servers.SingleOrDefault(x => x.StopServer) ?? servers.First();
                if (GetServerState(manuelTestServer) == LoadBalanceState.Offline)
                    return;

                BringOffline(manuelTestServer, status, settings, _loadBalancer, token);
                SetServerState(manuelTestServer, LoadBalanceState.Offline);
                //ExecuteOnServer(manuelTestServer, status, settings, _loadBalancer, true, false, token);
                return;
            }

            if (settings.Options.ContinueAfterMarkedServer)
            {
                manuelTestServer = servers.SingleOrDefault(x => x.StopServer) ?? servers.First();
                //servers.Remove(manuelTestServer);

                if (manuelTestServer.Name.Equals(server.Name))
                {
                    return;
                }

                //if (roundRobinMaxOfflineServers == 1)
                //{
                //    _loadBalancer.BringOnline(manuelTestServer.Name, manuelTestServer.LoadBalancerFarm, status);
                //}
                BringOffline(server, status, settings, _loadBalancer, token);
                SetServerState(server, LoadBalanceState.Offline);
                _serversToKeepOffline.Add(server.Name, LoadBalanceState.Offline);
                return;
            }

            if (_servers.Count() == 1)
            {
                BringOffline(server, status, settings, _loadBalancer, token);
                SetServerState(server, LoadBalanceState.Offline);
                return;
            }

            var activeServerIndex = _servers.ToList().IndexOf(server);
            //for (int execCount = 0; execCount < servers.Count; execCount++)
            //{
            if (activeServerIndex == roundRobinMaxOfflineServers - (manuelTestServer == null ? 0 : 1))
            {
                TurnRoundRobinServersAround(_loadBalancer, servers, settings, token, roundRobinMaxOfflineServers,
                    manuelTestServer, status);
            }
            else
            {
                BringOffline(server, status, settings, _loadBalancer, token);
                SetServerState(server, LoadBalanceState.Offline);
                _serversToKeepOffline.Add(server.Name, LoadBalanceState.Offline);
            }


            //if (servers.Count == 1)
            //{
            //    ExecuteOnServer(servers.First(), status, settings, _loadBalancer, true, true, token);
            //    return;
            //}

            //for (int execCount = 0; execCount < servers.Count; execCount++)
            //{
            //    if (execCount == roundRobinMaxOfflineServers - (manuelTestServer == null ? 0 : 1))
            //    {
            //        TurnRoundRobinServersAround(_loadBalancer, servers, roundRobinMaxOfflineServers, manuelTestServer, status);
            //    }

            //    bool bringOnline = !(roundRobinMaxOfflineServers - (manuelTestServer == null ? 0 : 1) > execCount);
            //    ExecuteOnServer(servers[execCount], status, settings, _loadBalancer, !bringOnline, bringOnline, token);
            //}
        }

        public override void BringOnline(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            if (GetServerState(server) == LoadBalanceState.Online)
                return;

            if (_serversToKeepOffline.ContainsKey(server.Name))
                return;

            if (settings.Options.StopAfterMarkedServer)
                return;

            var servers = _servers.ToList();
            var roundRobinMaxOfflineServers = (int)Math.Ceiling(((double)servers.Count) / 2);
            ServerConfig manuelTestServer = null;

            if (settings.Options.ContinueAfterMarkedServer)
            {
                manuelTestServer = servers.SingleOrDefault(x => x.StopServer) ?? servers.First();
                servers.Remove(manuelTestServer);

                if (roundRobinMaxOfflineServers == 1)
                {
                    BringOnline(manuelTestServer, status, settings, _loadBalancer, token);
                    return;
                    //_loadBalancer.BringOnline(manuelTestServer.Name, manuelTestServer.LoadBalancerFarm, status);
                }
            }

            if (servers.Count == 1)
            {
                BringOnline(server, status, settings, _loadBalancer, token);
                //ExecuteOnServer(servers.First(), status, settings, _loadBalancer, true, true, token);
                return;
            }

            if (!_serversToKeepOffline.ContainsKey(server.Name))
            {
                BringOnline(server, status, settings, _loadBalancer, token);
            }

            //var activeServerIndex = _servers.ToList().IndexOf(server) + 1;
            ////for (int execCount = 0; execCount < servers.Count; execCount++)
            ////{
            //if (activeServerIndex == roundRobinMaxOfflineServers - (manuelTestServer == null ? 0 : 1))
            //{
            //    TurnRoundRobinServersAround(_loadBalancer, servers, settings, token, roundRobinMaxOfflineServers,
            //        manuelTestServer, status);
            //}

                //bool bringOnline = !(roundRobinMaxOfflineServers - (manuelTestServer == null ? 0 : 1) > execCount);
                //ExecuteOnServer(servers[execCount], status, settings, _loadBalancer, !bringOnline, bringOnline, token);
            //}
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConDep.Dsl.Builders;
using ConDep.Dsl.Config;
using ConDep.Dsl.LoadBalancer;

namespace ConDep.Dsl
{
    public class OperationsBuilder : IOfferOperations
    {
        private readonly int _serial;
        private readonly ConDepSettings _settings;
        private readonly ILookupLoadBalancer _loadBalancerLocator;
        private readonly CancellationToken _token;

        public OperationsBuilder(int serial, ConDepSettings settings, ILookupLoadBalancer loadBalancerLocator, CancellationToken token)
        {
            _serial = serial;
            _settings = settings;
            _loadBalancerLocator = loadBalancerLocator;
            _token = token;
        }


        public IOfferOperations LoadBalance(LoadBalancerMode mode, string farm, Action<IOfferRemoteOperations> remote)
        {
            return ExecuteRemoteWithLoadBalancer(mode, remote, _settings.Config.Servers.ToList(), farm);
        }

        public IOfferOperations LoadBalance(LoadBalancerMode mode, string farm, Action<IOfferRemoteOperations, ServerConfig> remote)
        {
            return ExecuteRemoteWithLoadBalancer(mode, remote, _settings.Config.Servers.ToList(), farm);
        }

        public IOfferOperations LoadBalance(LoadBalancerMode mode, Tier tier, string farm, Action<IOfferRemoteOperations> remote)
        {
            return ExecuteRemoteWithLoadBalancer(mode, remote, _settings.Config.Servers.ToList(), farm);
        }

        public IOfferOperations LoadBalance(LoadBalancerMode mode, Tier tier, string farm, Action<IOfferRemoteOperations, ServerConfig> remote)
        {
            return ExecuteRemoteWithLoadBalancer(mode, remote, _settings.Config.Servers.ToList(), farm);
        }

        public IOfferOperations LoadBalance(LoadBalancerMode mode, string tier, string farm, Action<IOfferRemoteOperations> remote)
        {
            return ExecuteRemoteWithLoadBalancer(mode, remote, _settings.Config.Servers.ToList(), farm);
        }

        public IOfferOperations LoadBalance(LoadBalancerMode mode, string tier, string farm, Action<IOfferRemoteOperations, ServerConfig> remote)
        {
            return ExecuteRemoteWithLoadBalancer(mode, remote, _settings.Config.Servers.ToList(), farm);
        }

        public IOfferOperations Remote(Action<IOfferRemoteOperations> remote)
        {
            return ExecuteRemote(remote, _settings.Config.Servers);
        }


        public IOfferOperations Remote(Action<IOfferRemoteOperations, ServerConfig> remote)
        {
            return ExecuteRemote(remote, _settings.Config.Servers);
        }

        public async Task<IOfferOperations> RemoteAsync(Action<IOfferRemoteOperations, ServerConfig> remote)
        {
            return await ExecuteRemoteAsync(remote, _settings.Config.Servers);
        }

        public async Task<IOfferOperations> RemoteAsync(Action<IOfferRemoteOperations> remote)
        {
            return await ExecuteRemoteAsync(remote, _settings.Config.Servers);
        }

        public IOfferOperations Remote(Tier tier, Action<IOfferRemoteOperations> remote)
        {
            var servers = _settings.Config.Tiers.Single(x => x.Name.Equals(tier.ToString())).Servers;
            return ExecuteRemote(remote, servers);
        }

        public IOfferOperations Remote(Tier tier, Action<IOfferRemoteOperations, ServerConfig> remote)
        {
            var servers = _settings.Config.Tiers.Single(x => x.Name.Equals(tier.ToString())).Servers;
            return ExecuteRemote(remote, servers);
        }

        public IOfferOperations Remote(string tier, Action<IOfferRemoteOperations> remote)
        {
            var servers = _settings.Config.Tiers.Single(x => x.Name.Equals(tier)).Servers;
            return ExecuteRemote(remote, servers);
        }

        public IOfferOperations Remote(string tier, Action<IOfferRemoteOperations, ServerConfig> remote)
        {
            var servers = _settings.Config.Tiers.Single(x => x.Name.Equals(tier)).Servers;
            return ExecuteRemote(remote, servers);
        }

        public async Task<IOfferOperations> RemoteAsync(Tier tier, Action<IOfferRemoteOperations> remote)
        {
            var servers = _settings.Config.Tiers.Single(x => x.Name.Equals(tier.ToString())).Servers;
            return await ExecuteRemoteAsync(remote, servers);
        }

        public async Task<IOfferOperations> RemoteAsync(string tier, Action<IOfferRemoteOperations> remote)
        {
            var servers = _settings.Config.Tiers.Single(x => x.Name.Equals(tier)).Servers;
            return await ExecuteRemoteAsync(remote, servers);
        }

        public async Task<IOfferOperations> RemoteAsync(Tier tier, Action<IOfferRemoteOperations, ServerConfig> remote)
        {
            var servers = _settings.Config.Tiers.Single(x => x.Name.Equals(tier.ToString())).Servers;
            return await ExecuteRemoteAsync(remote, servers);
        }

        public async Task<IOfferOperations> RemoteAsync(string tier, Action<IOfferRemoteOperations, ServerConfig> remote)
        {
            var servers = _settings.Config.Tiers.Single(x => x.Name.Equals(tier)).Servers;
            return await ExecuteRemoteAsync(remote, servers);
        }

        private IOfferOperations ExecuteRemote(Action<IOfferRemoteOperations, ServerConfig> remote, IList<ServerConfig> servers)
        {
            foreach (var chunk in servers.Chunk(_serial))
            {
                Task.Run(() =>
                {
                    foreach (var server in chunk)
                    {
                        var builder = new RemoteOperationsBuilder(server, _settings, _token);
                        remote(builder, server);
                        Result = builder.Result;
                    }
                }, _token).Wait(_token);
            }
            return this;
        }

        private IOfferOperations ExecuteRemote(Action<IOfferRemoteOperations> remote, IList<ServerConfig> servers)
        {
            foreach (var chunk in servers.Chunk(_serial))
            {
                Task.Run(() =>
                {
                    foreach (
                        var builder in chunk.Select(server => new RemoteOperationsBuilder(server, _settings, _token)))
                    {
                        remote(builder);
                        Result = builder.Result;
                    }
                }, _token).Wait(_token);
            }

            return this;
        }

        private async Task<IOfferOperations> ExecuteRemoteAsync(Action<IOfferRemoteOperations, ServerConfig> remote, IList<ServerConfig> servers)
        {
            await Task.Run(() =>
            {
                foreach (var chunk in servers.Chunk(_serial))
                {
                    Task.Run(() =>
                    {
                        foreach (var server in chunk)
                        {
                            var builder = new RemoteOperationsBuilder(server, _settings, _token);
                            remote(builder, server);
                            Result = builder.Result;
                        }
                    }, _token).Wait(_token);
                }
            }, _token);
            return this;
        }

        private async Task<IOfferOperations> ExecuteRemoteAsync(Action<IOfferRemoteOperations> remote, IList<ServerConfig> servers)
        {
            await Task.Run(() =>
            {
                foreach (var chunk in servers.Chunk(_serial))
                {
                    Task.Run(() =>
                    {
                        foreach (
                            var builder in
                                chunk.Select(server => new RemoteOperationsBuilder(server, _settings, _token)))
                        {
                            remote(builder);
                            Result = builder.Result;
                        }
                    }, _token).Wait(_token);
                }
            }, _token);

            return this;
        }


        private IOfferOperations ExecuteRemoteWithLoadBalancer(LoadBalancerMode mode, Action<IOfferRemoteOperations, ServerConfig> remote, List<ServerConfig> servers, string farm)
        {
            var lbExecutor = GetLoadBalancer(mode, servers);

            var errorDuringLoadBalancing = false;
            foreach (var server in lbExecutor.GetServerExecutionOrder(servers, _settings, _token))
            {
                try
                {
                    lbExecutor.BringOffline(server, _settings, _token);

                    if (!server.LoadBalancerState.PreventDeployment)
                    {
                        ExecuteOperation(remote, server);
                    }
                }
                catch
                {
                    errorDuringLoadBalancing = true;
                    throw;
                }
                finally
                {
                    if (!errorDuringLoadBalancing && !_settings.Options.StopAfterMarkedServer)
                    {
                        lbExecutor.BringOnline(server, _settings, _token);
                    }
                }

            }

            return this;
        }

        private IOfferOperations ExecuteRemoteWithLoadBalancer(LoadBalancerMode mode, Action<IOfferRemoteOperations> remote, List<ServerConfig> servers, string farm)
        {
            var lbExecutor = GetLoadBalancer(mode, servers);

            var errorDuringLoadBalancing = false;
            foreach (var server in lbExecutor.GetServerExecutionOrder(servers, _settings, _token))
            {
                try
                {
                    lbExecutor.BringOffline(server, _settings, _token);

                    if (!server.LoadBalancerState.PreventDeployment)
                    {
                        ExecuteOperation(remote, server);
                    }
                }
                catch
                {
                    errorDuringLoadBalancing = true;
                    throw;
                }
                finally
                {
                    if (!errorDuringLoadBalancing && !_settings.Options.StopAfterMarkedServer)
                    {
                        lbExecutor.BringOnline(server, _settings, _token);
                    }
                }

            }
            return this;
        }

        private LoadBalancerExecutorBase GetLoadBalancer(LoadBalancerMode mode, IList<ServerConfig> servers)
        {
            LoadBalancerExecutorBase lbExecutor = null;
            var loadBalancer = _loadBalancerLocator.GetLoadBalancer();

            switch (mode)
            {
                case LoadBalancerMode.RoundRobin:
                    lbExecutor = new RoundRobinLoadBalancerExecutor(servers, loadBalancer);
                    break;
                case LoadBalancerMode.Sticky:
                    lbExecutor = new StickyLoadBalancerExecutor(loadBalancer);
                    break;
                case LoadBalancerMode.OfflinePriority:
                    lbExecutor = new OfflinePriorityLoadBalancerExecutor(loadBalancer);
                    break;
                default:
                    throw new ConDepLoadBalancerException("Load balancer mode not supported");
            }
            return lbExecutor;
        }

        private void ExecuteOperation(Action<IOfferRemoteOperations> remote, ServerConfig server)
        {
            var builder = new RemoteOperationsBuilder(server, _settings, _token);
            remote(builder);
            Result = builder.Result;
        }

        private void ExecuteOperation(Action<IOfferRemoteOperations, ServerConfig> remote, ServerConfig server)
        {
            var builder = new RemoteOperationsBuilder(server, _settings, _token);
            remote(builder, server);
            Result = builder.Result;
        }


        public IOfferLocalOperations Local => new LocalOperationsBuilder(_settings, _token);

        public Result Result { get; set; }
    }
}
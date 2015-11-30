using System;
using System.Threading;
using System.Threading.Tasks;
using ConDep.Dsl.Builders;
using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    public class OperationsBuilder : IOfferOperations
    {
        private readonly int _serial;
        private readonly ConDepSettings _settings;
        private readonly CancellationToken _token;

        public OperationsBuilder(int serial, ConDepSettings settings, CancellationToken token)
        {
            _serial = serial;
            _settings = settings;
            _token = token;
        }

        public IOfferOperations Remote(Action<IOfferRemoteOperations> remote)
        {
            foreach (var chunk in _settings.Config.Servers.Chunk(_serial))
            {
                Task.Run(() =>
                {
                    foreach (var server in chunk)
                    {
                        var builder = new RemoteOperationsBuilder(server, _settings, _token);
                        remote(builder);
                        Result = builder.Result;
                    }
                }).Wait();
            }
            return this;
        }

        public IOfferOperations Remote(Action<IOfferRemoteOperations, ServerConfig> remote)
        {
            foreach (var chunk in _settings.Config.Servers.Chunk(_serial))
            {
                Task.Run(() =>
                {
                    foreach (var server in chunk)
                    {
                        var builder = new RemoteOperationsBuilder(server, _settings, _token);
                        remote(builder, server);
                        Result = builder.Result;
                    }
                }).Wait();
            }

            return this;
        }

        public IOfferOperations Remote(Action<IOfferRemoteOperations, ServerConfig, ConDepSettings> remote)
        {
            foreach (var chunk in _settings.Config.Servers.Chunk(_serial))
            {
                Task.Run(() =>
                {
                    foreach (var server in chunk)
                    {
                        var builder = new RemoteOperationsBuilder(server, _settings, _token);
                        remote(builder, server, _settings);
                        Result = builder.Result;
                    }
                }).Wait();
            }

            return this;
        }

        public async Task<IOfferOperations> RemoteAsync(Action<IOfferRemoteOperations, ServerConfig> remote)
        {
            await Task.Run(() =>
            {
                foreach (var chunk in _settings.Config.Servers.Chunk(_serial))
                {
                    Task.Run(() =>
                    {
                        foreach (var server in chunk)
                        {
                            var builder = new RemoteOperationsBuilder(server, _settings, _token);
                            remote(builder, server);
                            Result = builder.Result;
                        }
                    }).Wait();
                }
            });

            return this;
        }

        public async Task<IOfferOperations> RemoteAsync(Action<IOfferRemoteOperations> remote)
        {
            await Task.Run(() =>
            {
                foreach (var chunk in _settings.Config.Servers.Chunk(_serial))
                {
                    Task.Run(() =>
                    {
                        foreach (var server in chunk)
                        {
                            var builder = new RemoteOperationsBuilder(server, _settings, _token);
                            remote(builder);
                            Result = builder.Result;
                        }
                    }).Wait();
                }
            });

            return this;
        }

        public IOfferLocalOperations Local
        {
            get
            {
                return new LocalOperationsBuilder(_settings, _token);
            }
        }

        public Result Result { get; set; }
    }
}
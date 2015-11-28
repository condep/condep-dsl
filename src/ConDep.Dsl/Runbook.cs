using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConDep.Dsl.Builders;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Remote;

namespace ConDep.Dsl
{
    [Obsolete("Artifact has been renamed to Runbook to avoid confusion. Please use Runbook instead.", true)]
    public class Artifact
    {
        [Obsolete("Artifact.Local has been renamed to Runbook.Local to avoid confusion. Please use Runbook.Local instead.", true)]
        public abstract class Local : IProvideRunbook
        {
            public abstract void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings);
            public IEnumerable<IProvideRunbook> Dependencies { get; set; }
        }

        [Obsolete("Artifact.Remote has been renamed to Runbook.Remote. Please use Runbook.Remote instead.", true)]
        public abstract class Remote : IProvideRunbook
        {
            public abstract void Configure(IOfferRemoteOperations server, ConDepSettings settings);
            public IEnumerable<IProvideRunbook> Dependencies { get; set; }
        }
    }

    /// <summary>
    /// Container class for <see cref="Runbook.Local"/> and <see cref="Runbook.Remote"/>.
    /// </summary>
    public abstract class Runbook
    {
        public abstract void Execute(IOfferOperations dsl);

        /// <summary>
        /// Use this <see cref="Runbook" /> to get access to ConDep's Local Operations DSL and perform local operations. 
        /// If you need access to the remote DSL, use the ToEachServer() method.
        /// </summary>
        public abstract class Local : IProvideRunbook
        {
            public abstract void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings);
            public IEnumerable<IProvideRunbook> Dependencies { get; set; }
        }

        /// <summary>
        /// Use this <see cref="Runbook"/> to get access to ConDep's Remote Operations DSL and perform remote operations. 
        /// If you need access to the local DSL, use the <see cref="Runbook.Local"/> class instead. This 
        /// will give you access to both the local and remote DSL if needed.
        /// </summary>
        public abstract class Remote : IProvideRunbook
        {
            public abstract void Configure(IOfferRemoteOperations server, ConDepSettings settings);
            public IEnumerable<IProvideRunbook> Dependencies { get; set; }
        }
    }

    public interface IOfferOperations
    {
        IOfferOperations Remote(Action<IOfferRemoteOperations> remote);
        IOfferOperations Remote(Action<IOfferRemoteOperations, ServerConfig> remote);
        IOfferOperations Remote(Action<IOfferRemoteOperations, ServerConfig, ConDepSettings> remote);
        Task<IOfferOperations> RemoteAsync(Action<IOfferRemoteOperations, ServerConfig> remote);
        Task<IOfferOperations> RemoteAsync(Action<IOfferRemoteOperations> remote);
        //IOfferOperations Remote(Tier tier, Action<IOfferRemoteOps> remote);
        //IOfferOperations Remote(string tier, Action<IOfferRemoteOps> remote);
        //IOfferOperations Remote(Tier tier, Action<IOfferRemoteOps, ServerConfig> remote);
        //IOfferOperations Remote(string tier, Action<IOfferRemoteOps, ServerConfig> remote);
        IOfferLocalOperations Local { get; }
    }

    internal class OperationsBuilder : IOfferOperations
    {
        private readonly int _serial;
        private readonly IEnumerable<ServerConfig> _servers;
        private readonly IEnumerable<Tier> _tiers;
        private readonly ConDepSettings _settings;

        public OperationsBuilder(int serial, IEnumerable<ServerConfig> servers, IEnumerable<Tier> tiers, ConDepSettings settings)
        {
            _serial = serial;
            _servers = servers;
            _tiers = tiers;
            _settings = settings;
        }

        public IOfferOperations Remote(Action<IOfferRemoteOperations> remote)
        {
            foreach (var chunk in _servers.Chunk(_serial))
            {
                Task.Run(() =>
                {
                    foreach (var server in chunk)
                    {
                        var builder = new RemoteOperationsBuilderNew(server);
                        remote(builder);
                        Result = builder.Result;
                    }
                }).Wait();
            }
            return this;
        }

        public IOfferOperations Remote(Action<IOfferRemoteOperations, ServerConfig> remote)
        {
            foreach (var chunk in _servers.Chunk(_serial))
            {
                Task.Run(() =>
                {
                    foreach (var server in chunk)
                    {
                        var builder = new RemoteOperationsBuilderNew(server);
                        remote(builder, server);
                        Result = builder.Result;
                    }
                }).Wait();
            }

            return this;
        }

        public IOfferOperations Remote(Action<IOfferRemoteOperations, ServerConfig, ConDepSettings> remote)
        {
            foreach (var chunk in _servers.Chunk(_serial))
            {
                Task.Run(() =>
                {
                    foreach (var server in chunk)
                    {
                        var builder = new RemoteOperationsBuilderNew(server);
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
                foreach (var chunk in _servers.Chunk(_serial))
                {
                    Task.Run(() =>
                    {
                        foreach (var server in chunk)
                        {
                            var builder = new RemoteOperationsBuilderNew(server);
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
                foreach (var chunk in _servers.Chunk(_serial))
                {
                    Task.Run(() =>
                    {
                        foreach (var server in chunk)
                        {
                            var builder = new RemoteOperationsBuilderNew(server);
                            remote(builder);
                            Result = builder.Result;
                        }
                    }).Wait();
                }
            });

            return this;
        }

        public IOfferLocalOperations Local { get; }

        public Result Result { get; set; }
    }

    public abstract class Operation
    {
        public abstract class ForEachServer : Operation
        {
            public abstract Result Execute(PowerShellExecutor remotePowerShell, ServerConfig server, ConDepSettings settings, CancellationToken token);
        }

        public abstract class RemoteComposite : Operation
        {
            public abstract Result Execute(IOfferRemoteOperations remote, ConDepSettings settings, CancellationToken token);
        }

        public abstract class RemoteCode : Operation
        {
            public abstract Result Execute(ServerConfig server, ConDepSettings settings, ILogForConDep logger, CancellationToken token);
        }

        public abstract class Local : Operation
        {
            public abstract Result Execute(ConDepSettings settings, CancellationToken token);
        }

        public abstract string Name { get; }
    }

    public class MyOp : Operation.ForEachServer
    {
        public override string Name { get; }

        public override Result Execute(PowerShellExecutor remotePowerShell, ServerConfig server, ConDepSettings settings, CancellationToken token)
        {
            remotePowerShell.Execute(server, "");
            return new Result();
        }
    }

    public class Result
    {
        public bool Success { get; set; }
        public bool Changed { get; set; }
        public string Output { get; set; }
    }

    public static class ChunkExtension
    {
        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }
    }
}
using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Builders
{
    public abstract class RemoteBuilder : IOfferResult
    {
        private readonly ServerConfig _server;
        private readonly ConDepSettings _settings;
        private readonly CancellationToken _token;

        public RemoteBuilder(ServerConfig server, ConDepSettings settings, CancellationToken token)
        {
            _server = server;
            _settings = settings;
            _token = token;
        }

        public ConDepSettings Settings
        {
            get { return _settings; }
        }

        public CancellationToken Token
        {
            get { return _token; }
        }

        public ServerConfig Server
        {
            get { return _server; }
        }

        public Result Result { get; set; }

        public abstract IOfferRemoteOperations Dsl { get; }
    }
}
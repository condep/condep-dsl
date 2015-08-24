using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    public class ConDepNodeUrl
    {
        private readonly ServerConfig _server;
        private readonly ConDepSettings _settings;
        const string NODE_LISTEN_URL = "https://{0}:{1}/ConDepNode/";

        public ConDepNodeUrl(ServerConfig server, ConDepSettings settings)
        {
            _server = server;
            _settings = settings;
        }

        public string RemoteUrl { get { return string.Format(NODE_LISTEN_URL, _server.Name, _server.Node.Port); } }
        public string ListenUrl { get { return string.Format(NODE_LISTEN_URL, "localhost", _server.Node.Port); } }
        public int Port { get { return _server.Node.Port.Value; } }
    }
}
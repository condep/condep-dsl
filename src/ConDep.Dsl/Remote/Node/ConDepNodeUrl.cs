using ConDep.Dsl.Config;

namespace ConDep.Dsl.Remote.Node
{
    public class ConDepNodeUrl
    {
        private readonly ServerConfig _server;
        const string NODE_LISTEN_URL = "https://{0}:{1}/ConDepNode/";

        public ConDepNodeUrl(ServerConfig server)
        {
            _server = server;
        }

        public string RemoteUrl => string.Format(NODE_LISTEN_URL, _server.Name, _server.Node.Port);
        public string ListenUrl => string.Format(NODE_LISTEN_URL, "localhost", _server.Node.Port);
        public int Port => _server.Node.Port.Value;
    }
}
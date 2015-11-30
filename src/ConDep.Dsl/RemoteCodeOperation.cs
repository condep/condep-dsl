using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    public abstract class RemoteCodeOperation 
    {
        public abstract Result Execute(ServerConfig server, ConDepSettings settings, CancellationToken token);
        public abstract string Name { get; }
    }
}
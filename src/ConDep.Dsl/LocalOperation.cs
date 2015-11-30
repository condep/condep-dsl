using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    public abstract class LocalOperation
    {
        public abstract Result Execute(ConDepSettings settings, CancellationToken token);
        public abstract string Name { get; }
    }
}
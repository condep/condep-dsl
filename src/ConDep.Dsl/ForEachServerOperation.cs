using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl
{
    public abstract class ForEachServerOperation : IExecuteRemotely
    {
        public abstract void Execute(IServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token);
        public abstract string Name { get; }
        public abstract bool IsValid(Notification notification);
        public void DryRun() { }
    }
}
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl
{
    public interface IExecuteRemotely : IValidate
    {
        void Execute(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token);
        string Name { get; }
        void DryRun();
    }
}
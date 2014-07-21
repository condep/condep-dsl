using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl
{
    public interface IExecute
    {
        void Execute(IReportStatus status, ConDepSettings settings, CancellationToken token);
        string Name { get; }
        void DryRun();
    }
}
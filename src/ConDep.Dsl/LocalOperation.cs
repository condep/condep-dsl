using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl
{
    public abstract class LocalOperation : IExecuteLocally
	{
        public abstract void Execute(IReportStatus status, ConDepSettings settings, CancellationToken token);
        public abstract string Name { get; }
        public void DryRun()
        {
            Logger.WithLogSection(Name, () => {});
        }

        public abstract bool IsValid(Notification notification);
	}
}
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Sequence
{
    public class ExecutionSequenceManager
    {
        private readonly ILoadBalance _loadBalancer;
        internal readonly List<LocalSequence> _sequence = new List<LocalSequence>();

        public ExecutionSequenceManager(ILoadBalance loadBalancer)
        {
            _loadBalancer = loadBalancer;
        }

        public LocalSequence NewLocalSequence(string name)
        {
            var sequence = new LocalSequence(name, _loadBalancer);
            _sequence.Add(sequence);
            return sequence;
        }

        public void Execute(IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            foreach (var localSequence in _sequence)
            {
                token.ThrowIfCancellationRequested();

                LocalSequence sequence = localSequence;
                Logger.WithLogSection(localSequence.Name, () => sequence.Execute(status, settings, token));
            }
        }

        public bool IsValid(Notification notification)
        {
            return _sequence.All(x => x.IsValid(notification));
        }

        public void DryRun()
        {
            foreach (var item in _sequence)
            {
                Logger.WithLogSection(item.Name, () => { item.DryRun(); });
            }
        }
    }
}
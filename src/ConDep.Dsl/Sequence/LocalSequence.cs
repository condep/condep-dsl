using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Operations.Application.Local;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Sequence
{
    public class LocalSequence : IManageSequence<LocalOperation>, IExecute
    {
        private readonly string _name;
        private readonly ILoadBalance _loadBalancer;
        internal readonly List<IExecute> _sequence = new List<IExecute>();
        private List<RemoteSequences> _remoteSequences = new List<RemoteSequences>();

        public LocalSequence(string name, ILoadBalance loadBalancer)
        {
            _name = name;
            _loadBalancer = loadBalancer;
        }

        public void Add(LocalOperation operation, bool addFirst = false)
        {

            if(addFirst)
            {
                _sequence.Insert(0, operation);
            }
            else
            {
                _sequence.Add(operation);
            }
        }

        public IEnumerable<RemoteSequence> RemoteSequence(IEnumerable<ServerConfig> servers, bool paralell = false)
        {
            var remoteSequences = new RemoteSequences(servers, _loadBalancer, paralell);
            _sequence.Add(remoteSequences);
            return remoteSequences.Sequenceses;
        }

        //public RemoteSequence NewRemoteConditionalSequence(IEnumerable<ServerConfig> servers, Predicate<ServerInfo> condition, bool expectedConditionResult, bool paralell)
        //{
        //    var sequence = new RemoteConditionalSequence(servers, _loadBalancer, condition, expectedConditionResult, paralell);
        //    _sequence.Add(sequence);
        //    return sequence;
        //}

        public void Execute(IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            foreach (var element in _sequence)
            {
                token.ThrowIfCancellationRequested();
                IExecute internalElement = element;
                if (internalElement is RemoteSequences)
                {
                    ((RemoteSequences)internalElement).ExecuteFirst(status, settings, token);
                }
                else
                {
                    Logger.WithLogSection(internalElement.Name, () => internalElement.Execute(status, settings, token));
                }
            }

            foreach (var element in _sequence)
            {
                token.ThrowIfCancellationRequested();
                IExecute internalElement = element;
                if (internalElement is RemoteSequences)
                {
                    ((RemoteSequences)internalElement).ExecuteRemaining(status, settings, token);
                }
            }
        }

        public string Name { get { return _name; } }

        public bool IsValid(Notification notification)
        {
            var isLocalOpsValid = _sequence.OfType<LocalOperation>().All(x => x.IsValid(notification));
            var isRemoteSeqValid = _sequence.OfType<RemoteSequence>().All(x => x.IsValid(notification));
            return isLocalOpsValid && isRemoteSeqValid;
        }

        public void DryRun()
        {
            foreach (var item in _sequence)
            {
                IExecute internalElement = item;
                if (internalElement is RemoteSequences)
                {
                    ((RemoteSequences) internalElement).DryRunFirst();
                }
                else
                {
                    internalElement.DryRun();
                }
            }

            foreach (var item in _sequence)
            {
                IExecute internalElement = item;
                if (internalElement is RemoteSequences)
                {
                    ((RemoteSequences)internalElement).DryRunRemaining();
                }
            }
        }
    }
}
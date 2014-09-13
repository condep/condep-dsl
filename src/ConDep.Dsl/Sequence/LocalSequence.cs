using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Operations.Application.Local;
using ConDep.Dsl.Operations.LoadBalancer;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Sequence
{
    public class LocalSequence : IManageSequence<LocalOperation>, IExecuteLocally
    {
        private readonly string _name;
        private readonly ExecutionSequenceManager _sequenceManager;
        private readonly ILoadBalance _loadBalancer;
        internal readonly List<IExecuteLocally> _sequence = new List<IExecuteLocally>();
        private LoadBalancerExecutorBase _internalLoadBalancer;

        public LocalSequence(string name, ExecutionSequenceManager sequenceManager, ILoadBalance loadBalancer)
        {
            _name = name;
            _sequenceManager = sequenceManager;
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

        public RemoteSequence NewRemoteSequence(string name, bool paralell = false)
        {
            return _sequenceManager.NewRemoteSequence(name);
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
                IExecuteLocally internalElement = element;
                Logger.WithLogSection(internalElement.Name, () => element.Execute(status, settings, token));
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
                item.DryRun();
            }
        }

        private LoadBalancerExecutorBase GetLoadBalancer(IEnumerable<ServerConfig> servers)
        {
            //if (_paralell)
            //{
            //    return new ParalellRemoteExecutor(_servers);
            //}

            switch (_loadBalancer.Mode)
            {
                case LbMode.Sticky:
                    return new StickyLoadBalancerExecutor(_loadBalancer);
                case LbMode.RoundRobin:
                    return new RoundRobinLoadBalancerExecutor(servers, _loadBalancer);
                default:
                    throw new ConDepLoadBalancerException(string.Format("Load Balancer mode [{0}] not supported.",
                                                                    _loadBalancer.Mode));
            }
            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations;
using ConDep.Dsl.Operations.LoadBalancer;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Sequence
{
    public class RemoteSequence : IManageRemoteSequence, IExecute
    {
        private readonly IEnumerable<ServerConfig> _servers;
        private readonly ILoadBalance _loadBalancer;
        private readonly bool _paralell;
        internal readonly List<IExecuteOnServer> _sequence = new List<IExecuteOnServer>();

        public RemoteSequence(IEnumerable<ServerConfig> servers, ILoadBalance loadBalancer, bool paralell = false)
        {
            _servers = servers;
            _loadBalancer = loadBalancer;
            _paralell = paralell;
        }

        public void Add(IExecuteOnServer operation, bool addFirst = false)
        {
            if (addFirst)
            {
                _sequence.Insert(0, operation);
            }
            else
            {
                _sequence.Add(operation);
            }
        }

        public virtual void Execute(IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            GetExecutor().Execute(status, settings, token);
        }

        private LoadBalancerExecutorBase GetExecutor()
        {
            if (_paralell)
            {
                return new ParalellRemoteExecutor(_sequence, _servers);
            }

            switch (_loadBalancer.Mode)
            {
                case LbMode.Sticky:
                    return new StickyLoadBalancerExecutor(_sequence, _servers, _loadBalancer);
                case LbMode.RoundRobin:
                    return new RoundRobinLoadBalancerExecutor(_sequence, _servers, _loadBalancer);
                default:
                    throw new ConDepLoadBalancerException(string.Format("Load Balancer mode [{0}] not supported.",
                                                                    _loadBalancer.Mode));
            }
        }

        public virtual string Name { get { return "Remote Operations"; } }

        public void DryRun()
        {
            GetExecutor().DryRun();
        }

        public CompositeSequence NewCompositeSequence(RemoteCompositeOperation operation)
        {
            var seq = new CompositeSequence(operation.Name);
            _sequence.Add(seq);
            return seq;
        }

        public CompositeSequence NewConditionalCompositeSequence(Predicate<ServerInfo> condition)
        {
            var sequence = new CompositeConditionalSequence(Name, condition, true);
            _sequence.Add(sequence);
            return sequence;
        }

        public bool IsValid(Notification notification)
        {
            var isRemoteOpValid = _sequence.OfType<IExecuteOnServer>().All(x => x.IsValid(notification));
            var isCompositeSeqValid = _sequence.OfType<CompositeSequence>().All(x => x.IsValid(notification));

            return isRemoteOpValid && isCompositeSeqValid;
        }
    }
}

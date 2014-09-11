using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Operations;
using ConDep.Dsl.Operations.LoadBalancer;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Sequence
{
    public class RemoteSequence : IManageRemoteSequence, IExecute
    {
        private readonly ServerConfig _server;
        private readonly ILoadBalance _loadBalancer;
        private readonly bool _paralell;
        internal readonly List<IExecuteOnServer> _sequence = new List<IExecuteOnServer>();

        public RemoteSequence(ServerConfig server, ILoadBalance loadBalancer, bool paralell = false)
        {
            _server = server;
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
            Logger.WithLogSection(_server.Name, () =>
            {
                foreach (var element in _sequence)
                {
                    token.ThrowIfCancellationRequested();

                    IExecuteOnServer elementToExecute = element;
                    if (element is CompositeSequence)
                        elementToExecute.Execute(_server, status, settings, token);
                    else
                        Logger.WithLogSection(element.Name, () => elementToExecute.Execute(_server, status, settings, token));
                }
            });

            //GetExecutor().Execute(status, settings, token);
        }

        private LoadBalancerExecutorBase GetLoadBalancer()
        {
            //if (_paralell)
            //{
            //    return new ParalellRemoteExecutor(_servers);
            //}

            //switch (_loadBalancer.Mode)
            //{
            //    case LbMode.Sticky:
            //        return new StickyLoadBalancerExecutor(_servers, _loadBalancer);
            //    case LbMode.RoundRobin:
            //        return new RoundRobinLoadBalancerExecutor(_servers, _loadBalancer);
            //    default:
            //        throw new ConDepLoadBalancerException(string.Format("Load Balancer mode [{0}] not supported.",
            //                                                        _loadBalancer.Mode));
            //}
            return null;
        }

        public virtual string Name { get { return "Remote Operations"; } }

        public void DryRun()
        {
            Logger.WithLogSection(_server.Name, () =>
            {
                foreach (var item in _sequence)
                {
                    IExecuteOnServer item1 = item;
                    Logger.WithLogSection(item.Name, item1.DryRun);
                }
            });

            //GetExecutor().DryRun();
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

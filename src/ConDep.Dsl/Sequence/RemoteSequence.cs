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
        private readonly bool _paralell;
        internal readonly List<IExecuteOnServer> _sequence = new List<IExecuteOnServer>();

        public RemoteSequence(ServerConfig server, bool paralell = false)
        {
            _server = server;
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
            Logger.WithLogSection(Server.Name, () =>
            {
                foreach (var element in _sequence)
                {
                    token.ThrowIfCancellationRequested();

                    IExecuteOnServer elementToExecute = element;
                    if (element is CompositeSequence)
                        elementToExecute.Execute(Server, status, settings, token);
                    else
                        Logger.WithLogSection(element.Name, () => elementToExecute.Execute(Server, status, settings, token));
                }
            });

            //GetExecutor().Execute(status, settings, token);
        }

        public virtual string Name { get { return "Remote Operations"; } }

        public ServerConfig Server
        {
            get { return _server; }
        }

        public void DryRun()
        {
            Logger.WithLogSection(Server.Name, () =>
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Operations;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Sequence
{
    public class RemoteSequence : IManageRemoteSequence, IExecuteRemotely
    {
        private readonly bool _paralell;
        internal readonly List<IExecuteRemotely> _sequence = new List<IExecuteRemotely>();

        public RemoteSequence(string name, bool paralell = false)
        {
            Name = name;
            _paralell = paralell;
        }

        public void Add(IExecuteRemotely operation, bool addFirst = false)
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

        public virtual void Execute(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            foreach (var element in _sequence)
            {
                token.ThrowIfCancellationRequested();

                IExecuteRemotely elementToExecute = element;
                if (element is CompositeSequence)
                    elementToExecute.Execute(server, status, settings, token);
                else
                    Logger.WithLogSection(element.Name, () => elementToExecute.Execute(server, status, settings, token));
            }
        }

        public virtual string Name { get; private set; }

        public void DryRun()
        {
            foreach (var item in _sequence)
            {
                IExecuteRemotely item1 = item;
                Logger.WithLogSection(item.Name, item1.DryRun);
            }
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
            var isRemoteOpValid = _sequence.OfType<IExecuteRemotely>().All(x => x.IsValid(notification));
            var isCompositeSeqValid = _sequence.OfType<CompositeSequence>().All(x => x.IsValid(notification));

            return isRemoteOpValid && isCompositeSeqValid;
        }
    }
}

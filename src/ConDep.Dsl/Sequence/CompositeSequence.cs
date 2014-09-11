using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Operations;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Sequence
{
    public class CompositeSequence : IManageRemoteSequence, IExecuteOnServer
    {
        private readonly string _compositeName;
        internal readonly List<IExecuteOnServer> _sequence = new List<IExecuteOnServer>();
        //private SequenceFactory _sequenceFactory;

        public CompositeSequence(string compositeName)
        {
            _compositeName = compositeName;
            //_sequenceFactory = new SequenceFactory(_sequence);
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

        public virtual void Execute(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            Logger.WithLogSection(_compositeName, () =>
                {
                    foreach (var element in _sequence)
                    {
                        token.ThrowIfCancellationRequested();

                        IExecuteOnServer elementToExecute = element;
                        if (element is CompositeSequence)
                            elementToExecute.Execute(server, status, settings, token);
                        else
                            Logger.WithLogSection(element.Name, () => elementToExecute.Execute(server, status, settings, token));

                    }
                });
        }

        public CompositeSequence NewCompositeSequence(RemoteCompositeOperation operation)
        {
            var seq = new CompositeSequence(operation.Name);
            _sequence.Add(seq);
            return seq;
            //return _sequenceFactory.NewCompositeSequence(operation);
        }

        public CompositeSequence NewConditionalCompositeSequence(Predicate<ServerInfo> condition)
        {
            var sequence = new CompositeConditionalSequence(Name, condition, true);
            _sequence.Add(sequence);
            return sequence;
        }

        public void DryRun()
        {
            foreach (var item in _sequence)
            {
                Logger.WithLogSection(item.Name, () => item.DryRun());
            }
        }

        public bool IsValid(Notification notification)
        {
            var isRemoteOpsValid = _sequence.OfType<IExecuteOnServer>().All(x => x.IsValid(notification));
            var isCompSeqValid = _sequence.OfType<CompositeSequence>().All(x => x.IsValid(notification));

            return isCompSeqValid && isRemoteOpsValid;
        }

        public virtual string Name { get { return _compositeName; } }
    }

    public class CompositeServersSequence : IManageRemoteSequence, IExecuteOnServer
    {
        private readonly string _compositeName;
        internal readonly List<IExecuteOnServer> _sequence = new List<IExecuteOnServer>();
        //private SequenceFactory _sequenceFactory;

        public CompositeServersSequence(string compositeName)
        {
            _compositeName = compositeName;
            //_sequenceFactory = new SequenceFactory(_sequence);
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

        public virtual void Execute(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            Logger.WithLogSection(_compositeName, () =>
            {
                foreach (var element in _sequence)
                {
                    token.ThrowIfCancellationRequested();

                    IExecuteOnServer elementToExecute = element;
                    if (element is CompositeSequence)
                        elementToExecute.Execute(server, status, settings, token);
                    else
                        Logger.WithLogSection(element.Name, () => elementToExecute.Execute(server, status, settings, token));

                }
            });
        }

        public CompositeSequence NewCompositeSequence(RemoteCompositeOperation operation)
        {
            var seq = new CompositeSequence(operation.Name);
            _sequence.Add(seq);
            return seq;
            //return _sequenceFactory.NewCompositeSequence(operation);
        }

        public CompositeSequence NewConditionalCompositeSequence(Predicate<ServerInfo> condition)
        {
            var sequence = new CompositeConditionalSequence(Name, condition, true);
            _sequence.Add(sequence);
            return sequence;
        }

        public void DryRun()
        {
            foreach (var item in _sequence)
            {
                Logger.WithLogSection(item.Name, () => item.DryRun());
            }
        }

        public bool IsValid(Notification notification)
        {
            var isRemoteOpsValid = _sequence.OfType<IExecuteOnServer>().All(x => x.IsValid(notification));
            var isCompSeqValid = _sequence.OfType<CompositeSequence>().All(x => x.IsValid(notification));

            return isCompSeqValid && isRemoteOpsValid;
        }

        public virtual string Name { get { return _compositeName; } }
    }
}
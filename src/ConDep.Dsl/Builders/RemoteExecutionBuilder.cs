using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    internal class RemoteExecutionBuilder : IOfferRemoteExecution, IConfigureRemoteExecution
    {
        private readonly IOfferRemoteSequence _remoteSequence;

        public RemoteExecutionBuilder(IOfferRemoteSequence remoteSequence)
        {
            _remoteSequence = remoteSequence;
        }

        public void AddOperation(IExecuteRemotely operation)
        {
            _remoteSequence.Add(operation);
        }

        public void AddOperation(RemoteCompositeOperation operation)
        {
            operation.Configure(new RemoteCompositeBuilder(_remoteSequence.NewCompositeSequence(operation)));
        }

        public IOfferRemoteExecution OnlyIf(Predicate<ServerInfo> condition)
        {
            return new RemoteExecutionBuilder(_remoteSequence.NewConditionalCompositeSequence(condition));
        }

        public IOfferRemoteExecution OnlyIf(string conditionScript)
        {
            return new RemoteExecutionBuilder(_remoteSequence.NewConditionalCompositeSequence(conditionScript));
        }
    }
}
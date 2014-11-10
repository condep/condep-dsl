using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    internal class RemoteInstallationBuilder : IOfferRemoteInstallation, IConfigureRemoteInstallation
    {
        private readonly IManageRemoteSequence _remoteSequence;

        public RemoteInstallationBuilder(IManageRemoteSequence remoteSequence)
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

        public IOfferRemoteInstallation OnlyIf(Predicate<ServerInfo> condition)
        {
            return new RemoteInstallationBuilder(_remoteSequence.NewConditionalCompositeSequence(condition));
        }

        public IOfferRemoteInstallation OnlyIf(string conditionScript)
        {
            throw new NotImplementedException();
        }
    }
}
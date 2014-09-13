using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Operations;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    public class RemoteDeploymentBuilder : IOfferRemoteDeployment, IConfigureRemoteDeployment
    {
        private readonly IManageRemoteSequence _remoteSequence;

        public RemoteDeploymentBuilder(IManageRemoteSequence remoteSequence)
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
    }
}
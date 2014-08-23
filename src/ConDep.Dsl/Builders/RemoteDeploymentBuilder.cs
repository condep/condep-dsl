using ConDep.Dsl.Operations;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Builders
{
    public class RemoteDeploymentBuilder : IOfferRemoteDeployment, IConfigureRemoteDeployment
    {
        private readonly IManageRemoteSequence _remoteSequence;

        public RemoteDeploymentBuilder(IManageRemoteSequence remoteSequence)
        {
            _remoteSequence = remoteSequence;
        }

        public void AddOperation(IExecuteOnServer operation)
        {
            _remoteSequence.Add(operation);    
        }

        public void AddOperation(RemoteCompositeOperation operation)
        {
            operation.Configure(new RemoteCompositeBuilder(_remoteSequence.NewCompositeSequence(operation)));
        }
    }
}
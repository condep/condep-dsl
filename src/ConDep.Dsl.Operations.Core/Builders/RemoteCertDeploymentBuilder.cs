using System.Runtime.ConstrainedExecution;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Builders
{
    public class RemoteCertDeploymentBuilder : IOfferRemoteCertDeployment
    {
        private readonly IOfferRemoteDeployment _remoteDeployment;

        public RemoteCertDeploymentBuilder(IOfferRemoteDeployment remoteDeployment)
        {
            _remoteDeployment = remoteDeployment;
        }

        public IOfferRemoteDeployment RemoteDeployment
        {
            get { return _remoteDeployment; }
        }
    }
}
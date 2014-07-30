namespace ConDep.Dsl.Operations.Builders
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
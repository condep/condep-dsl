using System;
using System.Security.Cryptography.X509Certificates;
using ConDep.Dsl.Operations.Application.Deployment.Certificate;
using ConDep.Dsl.Operations.Infrastructure.IIS.WebSite;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Builders
{
    public class SslInfrastructureBuilder : IOfferSslInfrastructure
    {
        private readonly IManageRemoteSequence _remoteSequence;
        private readonly InfrastructureBuilder _infrastructureBuilder;

        public SslInfrastructureBuilder(IManageRemoteSequence remoteSequence, InfrastructureBuilder infrastructureBuilder)
        {
            _remoteSequence = remoteSequence;
            _infrastructureBuilder = infrastructureBuilder;
        }

        public IOfferInfrastructure FromStore(X509FindType findType, string findValue)
        {
            var certOp = new CertificateFromStoreOperation(findType, findValue);
            var compositeSequence = _remoteSequence.NewCompositeSequence(certOp);
            certOp.Configure(new RemoteCompositeBuilder(compositeSequence));
            return _infrastructureBuilder;
        }

        public IOfferInfrastructure FromStore(X509FindType findType, string findValue, Action<IOfferCertificateOptions> options)
        {
            var certOpt = new CertificateOptions();
            options(certOpt);

            var certOp = new CertificateFromStoreOperation(findType, findValue, certOpt);
            var compositeSequence = _remoteSequence.NewCompositeSequence(certOp);
            certOp.Configure(new RemoteCompositeBuilder(compositeSequence));
            return _infrastructureBuilder;
        }

        public IOfferInfrastructure FromFile(string path, string password)
        {
            var certOp = new CertificateFromFileOperation(path, password);
            var compositeSequence = _remoteSequence.NewCompositeSequence(certOp);
            certOp.Configure(new RemoteCompositeBuilder(compositeSequence));
            return _infrastructureBuilder;
        }

        public IOfferInfrastructure FromFile(string path, string password, Action<IOfferCertificateOptions> options)
        {
            var certOpt = new CertificateOptions();
            options(certOpt);

            var certOp = new CertificateFromFileOperation(path, password, certOpt);
            var compositeSequence = _remoteSequence.NewCompositeSequence(certOp);
            certOp.Configure(new RemoteCompositeBuilder(compositeSequence));
            return _infrastructureBuilder;
        }
    }
}
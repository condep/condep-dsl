using System;
using System.Security.Cryptography.X509Certificates;
using ConDep.Dsl.Builders;
using ConDep.Dsl.Operations.Application.Deployment.Certificate;
using ConDep.Dsl.Operations.Infrastructure.IIS.WebSite;

namespace ConDep.Dsl
{
    public static class RemoteCertDeploymentExtensions
    {
        /// <summary>
        /// Will deploy certificate found by find type and find value from the local certificate store, to remote certificate store on server.
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <returns></returns>
        public static IOfferRemoteDeployment FromStore(this IOfferRemoteCertDeployment remoteCert, X509FindType findType, string findValue)
        {
            return FromStore(remoteCert, findType, findValue, null);
        }

        /// <summary>
        /// Will deploy certificate found by find type and find value from the local certificate store, to remote certificate store on server with provided options.
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IOfferRemoteDeployment FromStore(this IOfferRemoteCertDeployment remoteCert, X509FindType findType, string findValue, Action<IOfferCertificateOptions> options)
        {
            var certOptions = new CertificateOptions();
            if (options != null)
            {
                options(certOptions);
            }

            var remoteCertBuilder = ((RemoteCertDeploymentBuilder) remoteCert).RemoteDeployment;
            var certOp = new CertificateFromStoreOperation(findType, findValue, certOptions);
            Configure.Deployment(remoteCertBuilder, certOp);
            return remoteCertBuilder;
        }

        /// <summary>
        /// Will deploy certificate from local file path given correct password for private key, and deploy to certificate store on remote server.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static IOfferRemoteDeployment FromFile(this IOfferRemoteCertDeployment remoteCert, string path, string password)
        {
            return FromFile(remoteCert, path, password, null);
        }

        /// <summary>
        /// Will deploy certificate from local file path given correct password for private key, and deploy to certificate store on remote server with provided options.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="password"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IOfferRemoteDeployment FromFile(this IOfferRemoteCertDeployment remoteCert, string path, string password, Action<IOfferCertificateOptions> options)
        {
            var certOptions = new CertificateOptions();
            if (options != null)
            {
                options(certOptions);
            }

            var remoteCertBuilder = ((RemoteCertDeploymentBuilder)remoteCert).RemoteDeployment;
            var certOp = new CertificateFromFileOperation(path, password, certOptions);
            Configure.Deployment(remoteCertBuilder, certOp);
            return remoteCertBuilder;
        }
    }
}
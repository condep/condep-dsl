using System;
using System.Security.Cryptography.X509Certificates;
using ConDep.Dsl.Builders;
using ConDep.Dsl.Operations.Application.Deployment.Certificate;
using ConDep.Dsl.Operations.Builders;
using ConDep.Dsl.Operations.Infrastructure.IIS.WebSite;

namespace ConDep.Dsl
{
    public static class SslInfrastructureExtensions
    {
        /// <summary>
        /// Will deploy certificate found by find type and find value from the local certificate store, to remote certificate store on server.
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <returns></returns>
        public static IOfferInfrastructure FromStore(this IOfferSslInfrastructure sslInfra, X509FindType findType, string findValue)
        {
            var infraBuilder = ((SslInfrastructureBuilder) sslInfra).InfrastructureBuilder;
            var certOp = new CertificateFromStoreOperation(findType, findValue);
            Configure.Infrastructure(infraBuilder, certOp);
            return infraBuilder;
        }

        /// <summary>
        /// Will deploy certificate found by find type and find value from the local certificate store, to remote certificate store on server with provided options.
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IOfferInfrastructure FromStore(this IOfferSslInfrastructure sslInfra, X509FindType findType, string findValue, Action<IOfferCertificateOptions> options)
        {
            var infraBuilder = ((SslInfrastructureBuilder)sslInfra).InfrastructureBuilder;
            var certOpt = new CertificateOptions();
            options(certOpt);

            var certOp = new CertificateFromStoreOperation(findType, findValue, certOpt);
            Configure.Infrastructure(infraBuilder, certOp);
            return infraBuilder;
        }

        /// <summary>
        /// Will deploy certificate from local file path given correct password for private key, and deploy to certificate store on remote server.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static IOfferInfrastructure FromFile(this IOfferSslInfrastructure sslInfra, string path, string password)
        {
            var infraBuilder = ((SslInfrastructureBuilder)sslInfra).InfrastructureBuilder;
            var certOp = new CertificateFromFileOperation(path, password);
            Configure.Infrastructure(infraBuilder, certOp);
            return infraBuilder;
        }

        /// <summary>
        /// Will deploy certificate from local file path given correct password for private key, and deploy to certificate store on remote server with provided options.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="password"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IOfferInfrastructure FromFile(this IOfferSslInfrastructure sslInfra, string path, string password, Action<IOfferCertificateOptions> options)
        {
            var infraBuilder = ((SslInfrastructureBuilder)sslInfra).InfrastructureBuilder;
            var certOpt = new CertificateOptions();
            options(certOpt);

            var certOp = new CertificateFromFileOperation(path, password, certOpt);
            Configure.Infrastructure(infraBuilder, certOp);
            return infraBuilder;
        }
    }
}
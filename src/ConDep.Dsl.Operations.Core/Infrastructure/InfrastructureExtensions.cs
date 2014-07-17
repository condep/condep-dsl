using System;
using ConDep.Dsl.Operations.Infrastructure;
using ConDep.Dsl.Operations.Infrastructure.IIS;
using ConDep.Dsl.Operations.Infrastructure.IIS.AppPool;
using ConDep.Dsl.Operations.Infrastructure.IIS.WebApp;
using ConDep.Dsl.Operations.Infrastructure.IIS.WebSite;
using ConDep.Dsl.Operations.Windows;

namespace ConDep.Dsl
{
    public static class InfrastructureExtensions
    {
        /// <summary>
        /// Installs and configures IIS with provided options
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IOfferInfrastructure IIS(this IOfferInfrastructure infra, Action<IisInfrastructureOptions> options)
        {
            var op = new IisInfrastructureOperation();
            options(new IisInfrastructureOptions(op));

            Configure.InfrastructureOperations.AddOperation(op);
            return infra;
        }

        /// <summary>
        /// Installs IIS
        /// </summary>
        /// <returns></returns>
        public static IOfferInfrastructure IIS(this IOfferInfrastructure infra)
        {
            var op = new IisInfrastructureOperation();
            Configure.InfrastructureOperations.AddOperation(op);
            return infra;
        }

        /// <summary>
        /// Offer common Windows operations
        /// </summary>
        /// <returns></returns>
        public static IOfferInfrastructure Windows(this IOfferInfrastructure infra, Action<WindowsInfrastructureOptions> options)
        {
            var op = new WindowsFeatureInfrastructureOperation();
            options(new WindowsInfrastructureOptions(op));
            Configure.InfrastructureOperations.AddOperation(op);
            return infra;
        }

        /// <summary>
        /// Creates a new Web Site in IIS if not exist. If exist, will delete and then create new.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IOfferInfrastructure IISWebSite(this IOfferInfrastructure infra, string name, int id)
        {
            var op = new IisWebSiteOperation(name, id);
            Configure.InfrastructureOperations.AddOperation(op);
            return infra;
        }

        /// <summary>
        /// Creates a new Web Site in IIS if not exist. If exist, will delete and then create new with provided options.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IOfferInfrastructure IISWebSite(this IOfferInfrastructure infra, string name, int id, Action<IOfferIisWebSiteOptions> options)
        {
            var opt = new IisWebSiteOptions();
            options(opt);
            var op = new IisWebSiteOperation(name, id, opt);
            Configure.InfrastructureOperations.AddOperation(op);
            return infra;
        }

        /// <summary>
        /// Will create a new Application Pool in IIS.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IOfferInfrastructure IISAppPool(this IOfferInfrastructure infra, string name)
        {
            var op = new IisAppPoolOperation(name);
            Configure.InfrastructureOperations.AddOperation(op);
            return infra;
        }

        /// <summary>
        /// Will create a new Application Pool in IIS with provided options.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IOfferInfrastructure IISAppPool(this IOfferInfrastructure infra, string name, Action<IOfferIisAppPoolOptions> options)
        {
            var opt = new IisAppPoolOptions();
            options(opt);
            var op = new IisAppPoolOperation(name, opt.Values);
            Configure.InfrastructureOperations.AddOperation(op);
            return infra;
        }

        /// <summary>
        /// Will create a new Web Application in IIS under the given Web Site.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="webSite"></param>
        /// <returns></returns>
        public static IOfferInfrastructure IISWebApp(this IOfferInfrastructure infra, string name, string webSite)
        {
            var op = new IisWebAppOperation(name, webSite);
            Configure.InfrastructureOperations.AddOperation(op);
            return infra;
        }

        /// <summary>
        /// Will create a new Web Application in IIS under the given Web Site, with the provided options.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="webSite"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IOfferInfrastructure IISWebApp(this IOfferInfrastructure infra, string name, string webSite, Action<IOfferIisWebAppOptions> options)
        {
            var op = new IisWebAppOperation(name, webSite);
            Configure.InfrastructureOperations.AddOperation(op);
            return infra;
        }

        /// <summary>
        /// Provide operations for installing SSL certificates.
        /// </summary>
        //public IOfferSslInfrastructure SslCertificate { get { return new SslInfrastructureBuilder(_remoteSequence, this); } }

    }
}
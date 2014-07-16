using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations;
using ConDep.Dsl.Operations.Infrastructure;
using ConDep.Dsl.Operations.Infrastructure.IIS;
using ConDep.Dsl.Operations.Infrastructure.IIS.AppPool;
using ConDep.Dsl.Operations.Infrastructure.IIS.WebApp;
using ConDep.Dsl.Operations.Infrastructure.IIS.WebSite;
using ConDep.Dsl.Operations.Windows;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Builders
{
    public class InfrastructureBuilder : IOfferInfrastructure, IConfigureInfrastructure
    {
        private readonly IManageRemoteSequence _remoteSequence;

        public InfrastructureBuilder(IManageRemoteSequence remoteSequence)
        {
            _remoteSequence = remoteSequence;
        }

        public IOfferInfrastructure IIS(Action<IisInfrastructureOptions> options)
        {
            var op = new IisInfrastructureOperation();
            options(new IisInfrastructureOptions(op));

            AddOperation(op);
            return this;
        }

        public IOfferInfrastructure IIS()
        {
            var op = new IisInfrastructureOperation();
            AddOperation(op);
            return this;
        }

        public IOfferInfrastructure Windows(Action<WindowsInfrastructureOptions> options)
        {
            var op = new WindowsFeatureInfrastructureOperation();
            options(new WindowsInfrastructureOptions(op));
            AddOperation(op);
            return this;
        }

        public IOfferInfrastructure IISWebSite(string name, int id)
        {
            var op = new IisWebSiteOperation(name, id);
            AddOperation(op);
            return this;
        }

        public IOfferInfrastructure IISWebSite(string name, int id, Action<IOfferIisWebSiteOptions> options)
        {
            var opt = new IisWebSiteOptions();
            options(opt);
            var op = new IisWebSiteOperation(name, id, opt);
            AddOperation(op);
            return this;
        }

        public IOfferInfrastructure IISAppPool(string name)
        {
            var op = new IisAppPoolOperation(name);
            AddOperation(op);
            return this;
        }

        public IOfferInfrastructure IISAppPool(string name, Action<IOfferIisAppPoolOptions> options)
        {
            var opt = new IisAppPoolOptions();
            options(opt);
            var op = new IisAppPoolOperation(name, opt.Values);
            AddOperation(op);
            return this;
        }

        public IOfferInfrastructure IISWebApp(string name, string webSite)
        {
            var op = new IisWebAppOperation(name, webSite);
            AddOperation(op);
            return this;
        }

        public IOfferInfrastructure IISWebApp(string name, string webSite, Action<IOfferIisWebAppOptions> options)
        {
            var op = new IisWebAppOperation(name, webSite);
            AddOperation(op);
            return this;
        }

        public IOfferSslInfrastructure SslCertificate { get { return new SslInfrastructureBuilder(_remoteSequence, this); } }

        public IOfferInfrastructure OnlyIf(Predicate<ServerInfo> condition)
        {
            return new InfrastructureBuilder(_remoteSequence.NewConditionalCompositeSequence(condition));
        }

        public void AddOperation(RemoteCompositeOperation operation)
        {
            operation.Configure(new RemoteCompositeBuilder(_remoteSequence.NewCompositeSequence(operation)));
        }
    }
}
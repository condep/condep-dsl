using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Builders
{
    public class SslInfrastructureBuilder : IOfferSslInfrastructure
    {
        private readonly IOfferInfrastructure _infraBuilder;

        public SslInfrastructureBuilder(IOfferInfrastructure infraBuilder)
        {
            _infraBuilder = infraBuilder;
        }

        public IOfferInfrastructure InfrastructureBuilder
        {
            get { return _infraBuilder; }
        }
    }
}
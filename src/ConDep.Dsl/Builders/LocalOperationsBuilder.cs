using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Builders
{
    public class LocalOperationsBuilder : LocalBuilder, IOfferLocalOperations
    {
        public LocalOperationsBuilder(ConDepSettings settings, CancellationToken token) : base(settings, token)
        {
        }

        public override IOfferLocalOperations Dsl  => this;
    }
}
using System;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    public class LocalOperationsBuilder : LocalBuilder, IOfferLocalOperations
    {
        public LocalOperationsBuilder(ConDepSettings settings, CancellationToken token) : base(settings, token)
        {
        }
    }
}
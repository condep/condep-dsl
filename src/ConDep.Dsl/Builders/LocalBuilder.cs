using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Builders
{
    public abstract class LocalBuilder : IOfferResult
    {
        protected LocalBuilder(ConDepSettings settings, CancellationToken token)
        {
            Settings = settings;
            Token = token;
        }

        public ConDepSettings Settings { get; }

        public CancellationToken Token { get; }

        public Result Result { get; set; }

        public abstract IOfferLocalOperations Dsl { get; }
    }
}
using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Builders
{
    public class LocalBuilder : IOfferResult
    {
        public LocalBuilder(ConDepSettings settings, CancellationToken token)
        {
            Settings = settings;
            Token = token;
        }

        public ConDepSettings Settings { get; }

        public CancellationToken Token { get; }

        public Result Result { get; set; }
    }
}
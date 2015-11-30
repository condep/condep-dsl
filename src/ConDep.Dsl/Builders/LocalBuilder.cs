using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Builders
{
    public class LocalBuilder : IOfferResult
    {
        private readonly ConDepSettings _settings;
        private readonly CancellationToken _token;

        public LocalBuilder(ConDepSettings settings, CancellationToken token)
        {
            _settings = settings;
            _token = token;
        }

        public ConDepSettings Settings
        {
            get { return _settings; }
        }

        public CancellationToken Token
        {
            get { return _token; }
        }

        public Result Result { get; set; }
    }
}
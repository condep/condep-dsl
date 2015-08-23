using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Sequence
{
    public interface IOfferLocalSequence : IExecuteLocally
    {
        void Add(LocalOperation operation, bool addFirst = false);
        IOfferRemoteSequence NewRemoteSequence(string name, bool paralell = false);
    }
}
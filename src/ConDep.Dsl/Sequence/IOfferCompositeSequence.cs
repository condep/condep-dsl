using System;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Sequence
{
    internal interface IOfferCompositeSequence : IOfferRemoteSequence
    {
        void Execute(ServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token);
        string Name { get; }
    }
}
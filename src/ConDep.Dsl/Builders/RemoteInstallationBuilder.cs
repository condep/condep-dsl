using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Operations;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Builders
{
    public class RemoteInstallationBuilder : IOfferRemoteInstallation, IConfigureRemoteInstallation
    {
        private readonly List<IManageRemoteSequence> _remoteSequences;

        public RemoteInstallationBuilder(IEnumerable<IManageRemoteSequence> remoteSequences)
        {
            _remoteSequences = remoteSequences.ToList();
        }

        public void AddOperation(IExecuteOnServer operation)
        {
            foreach (var sequence in _remoteSequences)
            {
                sequence.Add(operation);
            }
        }

        public void AddOperation(RemoteCompositeOperation operation)
        {
            var sequences = _remoteSequences.Select(sequence => sequence.NewCompositeSequence(operation));
            operation.Configure(new RemoteCompositeBuilder(sequences));
        }
    }
}
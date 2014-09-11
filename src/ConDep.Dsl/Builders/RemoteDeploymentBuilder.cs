using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Operations;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    public class RemoteDeploymentBuilder : IOfferRemoteDeployment, IConfigureRemoteDeployment
    {
        private readonly IEnumerable<IManageRemoteSequence> _remoteSequences;

        public RemoteDeploymentBuilder(IEnumerable<IManageRemoteSequence> remoteSequences)
        {
            _remoteSequences = remoteSequences;
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
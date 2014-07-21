using System;
using System.Collections.Generic;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations.Application.Local;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    public class LocalOperationsBuilder : IOfferLocalOperations, IConfigureLocalOperations
    {
        private readonly LocalSequence _localSequence;
        private readonly IEnumerable<ServerConfig> _servers;

        public LocalOperationsBuilder(LocalSequence localSequence, IEnumerable<ServerConfig> servers)
        {
            _localSequence = localSequence;
            _servers = servers;
        }

        public IOfferLocalOperations ToEachServer(Action<IOfferRemoteOperations> action)
        {
            var builder = new RemoteOperationsBuilder(_localSequence.NewRemoteSequence(_servers));
            action(builder);
            return this;
        }

        public void AddOperation(LocalOperation operation)
        {
            _localSequence.Add(operation);
        }
    }
}
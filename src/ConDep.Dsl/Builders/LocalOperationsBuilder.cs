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

        public LocalOperationsBuilder(LocalSequence localSequence)
        {
            _localSequence = localSequence;
        }

        public IOfferLocalOperations ToEachServer(Action<IOfferRemoteOperations> action)
        {
            var builder = new RemoteOperationsBuilder(_localSequence.NewRemoteSequence(_localSequence.Name));
            action(builder);
            return this;
        }

        public IOfferLocalOperations ToEachServerInParalell(Action<IOfferRemoteOperations> action)
        {
            var builder = new RemoteOperationsBuilder(_localSequence.NewRemoteSequence(_localSequence.Name, true));
            action(builder);
            return this;
        }
        public void AddOperation(LocalOperation operation)
        {
            _localSequence.Add(operation);
        }
    }
}
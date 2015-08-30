using System;
using System.Collections.Generic;
using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    [Obsolete("Artifact has been deprecated. Use ExecutionPlan instead.", true)]
    public class Artifact
    {
        [Obsolete("Artifact.Local has been deprecated. Use ExecutionPlan.Local instead.", true)]
        public abstract class Local : IProvideExecutionPlan
        {
            public abstract void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings);
            public IEnumerable<IProvideExecutionPlan> Dependencies { get; set; }
        }

        [Obsolete("Artifact.Remote has been deprecated. Use ExecutionPlan.Remote instead.", true)]
        public abstract class Remote : IProvideExecutionPlan
        {
            public abstract void Configure(IOfferRemoteOperations server, ConDepSettings settings);
            public IEnumerable<IProvideExecutionPlan> Dependencies { get; set; }
        }
    }

    /// <summary>
    /// Container class for <see cref="ExecutionPlan.Local"/> and <see cref="ExecutionPlan.Remote"/>.
    /// </summary>
    public class ExecutionPlan
    {
        /// <summary>
        /// Use this <see chref="ExecutionPlan" /> to get access to ConDep's Local Operations DSL and perform local operations. 
        /// If you need access to the remote DSL, use the ToEachServer() method.
        /// </summary>
        public abstract class Local : IProvideExecutionPlan
        {
            public abstract void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings);
            public IEnumerable<IProvideExecutionPlan> Dependencies { get; set; }
        }

        /// <summary>
        /// Use this <see cref="ExecutionPlan"/> to get access to ConDep's Remote Operations DSL and perform remote operations. 
        /// If you need access to the local DSL, use the <see cref="ExecutionPlan.Local"/> class instead. This 
        /// will give you access to both the local and remote DSL.
        /// </summary>
        public abstract class Remote : IProvideExecutionPlan
        {
            public abstract void Configure(IOfferRemoteOperations server, ConDepSettings settings);
            public IEnumerable<IProvideExecutionPlan> Dependencies { get; set; }
        }
    }
}
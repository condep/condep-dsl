using System;
using System.Collections.Generic;
using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    [Obsolete("Artifact has been renamed to Runbook to avoid confusion. Please use Runbook instead.", true)]
    public class Artifact
    {
        [Obsolete("Artifact.Local has been renamed to Runbook.Local to avoid confusion. Please use Runbook.Local instead.", true)]
        public abstract class Local : IProvideRunbook
        {
            public abstract void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings);
            public IEnumerable<IProvideRunbook> Dependencies { get; set; }
        }

        [Obsolete("Artifact.Remote has been renamed to Runbook.Remote. Please use Runbook.Remote instead.", true)]
        public abstract class Remote : IProvideRunbook
        {
            public abstract void Configure(IOfferRemoteOperations server, ConDepSettings settings);
            public IEnumerable<IProvideRunbook> Dependencies { get; set; }
        }
    }

    /// <summary>
    /// Container class for <see cref="Runbook.Local"/> and <see cref="Runbook.Remote"/>.
    /// </summary>
    public class Runbook
    {
        /// <summary>
        /// Use this <see cref="Runbook" /> to get access to ConDep's Local Operations DSL and perform local operations. 
        /// If you need access to the remote DSL, use the ToEachServer() method.
        /// </summary>
        public abstract class Local : IProvideRunbook
        {
            public abstract void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings);
            public IEnumerable<IProvideRunbook> Dependencies { get; set; }
        }

        /// <summary>
        /// Use this <see cref="Runbook"/> to get access to ConDep's Remote Operations DSL and perform remote operations. 
        /// If you need access to the local DSL, use the <see cref="Runbook.Local"/> class instead. This 
        /// will give you access to both the local and remote DSL if needed.
        /// </summary>
        public abstract class Remote : IProvideRunbook
        {
            public abstract void Configure(IOfferRemoteOperations server, ConDepSettings settings);
            public IEnumerable<IProvideRunbook> Dependencies { get; set; }
        }
    }
}
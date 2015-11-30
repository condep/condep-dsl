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
}
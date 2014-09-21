using System.Collections.Generic;

namespace ConDep.Dsl
{
    public interface IProvideArtifact
    {
        IEnumerable<IProvideArtifact> Dependencies { get; set; } 
    }
}
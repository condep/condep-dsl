using System.Collections.Generic;

namespace ConDep.Dsl
{
    public interface IProvideExecutionPlan
    {
        IEnumerable<IProvideExecutionPlan> Dependencies { get; set; } 
    }
}
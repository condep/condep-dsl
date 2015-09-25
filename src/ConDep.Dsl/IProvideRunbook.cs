using System.Collections.Generic;

namespace ConDep.Dsl
{
    public interface IProvideRunbook
    {
        IEnumerable<IProvideRunbook> Dependencies { get; set; } 
    }
}
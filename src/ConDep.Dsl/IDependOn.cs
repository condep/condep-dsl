using System;

namespace ConDep.Dsl
{
    [Obsolete("This will be removed in future versions. " +
              "Instead of having a Runbook depend on another Runbook, " +
              "use the static method Runbook.Execute from within another Runbook instead.")]
    public interface IDependOn<T> where T : Runbook
    {
         
    }
}
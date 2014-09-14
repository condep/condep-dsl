using System;

namespace ConDep.Dsl.Execution
{
    [Serializable]
    public class TimedException
    {
        public DateTime DateTime { get; set; }
        public Exception Exception { get; set; }
    }
}
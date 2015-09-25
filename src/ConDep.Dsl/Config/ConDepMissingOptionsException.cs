using System;
using System.Collections.Generic;

namespace ConDep.Dsl.Config
{
    public class ConDepMissingOptionsException : Exception
    {
        public ConDepMissingOptionsException(IEnumerable<string> missingOptions) 
            : base("Missing mandatory options for " + string.Join(", ", missingOptions)) { }
    }
}
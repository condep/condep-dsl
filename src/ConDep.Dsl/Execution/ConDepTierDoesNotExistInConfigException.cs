using System;

namespace ConDep.Dsl.Execution
{
    public class ConDepTierDoesNotExistInConfigException : Exception
    {
        public ConDepTierDoesNotExistInConfigException(string message) : base(message)
        {
        }
    }
}
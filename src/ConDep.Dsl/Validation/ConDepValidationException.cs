using System;

namespace ConDep.Dsl.Validation
{
    public class ConDepValidationException : Exception
    {
        public ConDepValidationException(string message) : base(message) {}
    }
}
using System;

namespace ConDep.Dsl
{
    public class ConDepInvalidEnumValueException : Exception
    {
        public ConDepInvalidEnumValueException(Enum enumValue) : base(string.Format("Enum member [{0}] is currently not supported.", enumValue))
        {
                 
        }
    }
}
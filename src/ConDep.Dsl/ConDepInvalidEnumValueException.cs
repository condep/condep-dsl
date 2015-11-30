using System;

namespace ConDep.Dsl
{
    public class ConDepInvalidEnumValueException : Exception
    {
        public ConDepInvalidEnumValueException(Enum enumValue) : base($"Enum member [{enumValue}] is currently not supported.")
        {
                 
        }
    }
}
using System;

namespace ConDep.Dsl.Remote
{
    public class ConDepCredSSPException : Exception
    {
        public ConDepCredSSPException()
        {
        }

        public ConDepCredSSPException(string message) : base(message)
        {
        }
    }
}
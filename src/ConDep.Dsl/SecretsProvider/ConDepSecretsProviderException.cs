using System;

namespace ConDep.Dsl.SecretsProvider
{
    public class ConDepSecretsProviderException : Exception
    {
        public ConDepSecretsProviderException(string message) : base(message) { }
    }
}

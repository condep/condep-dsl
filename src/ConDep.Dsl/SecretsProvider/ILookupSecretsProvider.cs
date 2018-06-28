namespace ConDep.Dsl.SecretsProvider
{
    public interface ILookupSecretsProvider
    {
        IProvideSecrets GetSecretsProvider();
    }
}

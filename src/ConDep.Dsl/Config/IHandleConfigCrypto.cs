namespace ConDep.Dsl.Config
{
    public interface IHandleConfigCrypto<T>
    {
        T Decrypt(T config);
        T Encrypt(T config);
        bool IsEncrypted(string config);
    }
}
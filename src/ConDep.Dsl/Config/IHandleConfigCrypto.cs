namespace ConDep.Dsl.Config
{
    public interface IHandleConfigCrypto
    {
        string Decrypt(string config);
        void DecryptFile(string filePath);
        string Encrypt(string config);
        void EncryptFile(string filePath);
        bool IsEncrypted(string config);
    }
}
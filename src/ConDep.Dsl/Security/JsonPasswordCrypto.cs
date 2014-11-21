using System;
using System.Security.Cryptography;
using System.Text;

namespace ConDep.Dsl.Security
{
    public class JsonPasswordCrypto
    {
        private readonly string _key;

        public JsonPasswordCrypto(string key)
        {
            _key = key;
        }

        public static bool ValidKey(string key)
        {
            try
            {
                var aes = new AesManaged
                    {
                        Key = Convert.FromBase64String(key),
                        Mode = CipherMode.CBC,
                        Padding = PaddingMode.ISO10126
                    };
                aes.CreateEncryptor();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public EncryptedValue Encrypt(string password)
        {
            var aes = new AesManaged
                {
                    Key = Convert.FromBase64String(_key),
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.ISO10126
                };

            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var encryptor = aes.CreateEncryptor();

            var encryptedBytes = encryptor.TransformFinalBlock(passwordBytes, 0, passwordBytes.Length);

            return new EncryptedValue(Convert.ToBase64String(aes.IV), Convert.ToBase64String(encryptedBytes));
        }

        public EncryptedValue Encrypt(string key, string value)
        {
            var aes = new AesManaged
            {
                Key = Convert.FromBase64String(_key),
                Mode = CipherMode.CBC,
                Padding = PaddingMode.ISO10126
            };

            var valueBytes = Encoding.UTF8.GetBytes(value);
            var encryptor = aes.CreateEncryptor();

            var encryptedBytes = encryptor.TransformFinalBlock(valueBytes, 0, valueBytes.Length);

            return new EncryptedValue(Convert.ToBase64String(aes.IV), Convert.ToBase64String(encryptedBytes));
        }

        public string Decrypt(EncryptedValue encryptedValue)
        {
            var aes = new AesManaged
            {
                Key = Convert.FromBase64String(_key),
                Mode = CipherMode.CBC,
                Padding = PaddingMode.ISO10126,
                IV = Convert.FromBase64String(encryptedValue.IV)
            };

            var decryptor = aes.CreateDecryptor();
            var passwordBytes = Convert.FromBase64String(encryptedValue.Value);

            var decryptedBytes = decryptor.TransformFinalBlock(passwordBytes, 0, passwordBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public static string GenerateKey(int bitLength)
        {
            var byteLength = bitLength/8;
            var bytes = new byte[byteLength];
            new RNGCryptoServiceProvider().GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }

    }
}
using System.IO;
using System.Linq;
using ConDep.Dsl.Security;
using Newtonsoft.Json.Linq;

namespace ConDep.Dsl.Config
{
    public class JsonConfigCrypto : IHandleConfigCrypto
    {
        private readonly ISerializerConDepConfig _serializer;
        private readonly JsonPasswordCrypto _valueHandler;

        public JsonConfigCrypto(string key)
        {
            _valueHandler = new JsonPasswordCrypto(key);
        }

        public string Decrypt(string config)
        {
            var jsonConfig = JToken.Parse(config);
            Decrypt(jsonConfig);
            return jsonConfig.ToString();
        }

        public void DecryptFile(string filePath)
        {
            var json = GetConDepConfigAsJsonText(filePath);

            if (!IsEncrypted(json))
            {
                throw new ConDepCryptoException("Unable to decrypt. No content in file [{0}] is encrypted.");
            }

            var decryptedJson = Decrypt(json);
            UpdateConfig(filePath, decryptedJson);
        }

        private void Decrypt(JToken token)
        {
            token.FindEncryptedTokens()
                .ForEach(x => DecryptJsonValue(_valueHandler, x));
        }


        private void UpdateConfig(string filePath, string json)
        {
            File.WriteAllText(filePath, json);
        }

        private string GetConDepConfigAsJsonText(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("[{0}] not found.", filePath), filePath);
            }

            using (var fileStream = File.OpenRead(filePath))
            {
                using (var memStream = GetMemoryStreamWithCorrectEncoding(fileStream))
                {
                    using (var reader = new StreamReader(memStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
        public string Encrypt(string config)
        {
            var jsonConfig = JToken.Parse(config);

            jsonConfig.FindTaggedTokens("encrypt")
                .ForEach(x => EncryptTaggedValue(_valueHandler, x));

            var passwordTokens = jsonConfig.SelectTokens("$..Password").OfType<JValue>().ToList();
            foreach (var token in passwordTokens)
            {
                EncryptJsonValue(_valueHandler, token);
            }
            return jsonConfig.ToString();
        }

        public void EncryptFile(string filePath)
        {
            var json = GetConDepConfigAsJsonText(filePath);

            if (IsEncrypted(json))
                throw new ConDepCryptoException(string.Format("File [{0}] already encrypted.", filePath));

            var encryptedJson = Encrypt(json);
            UpdateConfig(filePath, encryptedJson);
        }

        public bool IsEncrypted(string jsonConfig)
        {
            var jsonModel = JObject.Parse(jsonConfig);

            var ivTokens = jsonModel.FindEncryptedTokens();

            return ivTokens
                .Select(token => token.ToObject<EncryptedValue>())
                .Any(value =>
                    value != null &&
                    !string.IsNullOrWhiteSpace(value.IV) &&
                    !string.IsNullOrWhiteSpace(value.Value)
                );
        }

        private static MemoryStream GetMemoryStreamWithCorrectEncoding(Stream stream)
        {
            using (var r = new StreamReader(stream, true))
            {
                var encoding = r.CurrentEncoding;
                return new MemoryStream(encoding.GetBytes(r.ReadToEnd()));
            }
        }

        private static void DecryptJsonValue(JsonPasswordCrypto cryptoHandler, dynamic originalValue)
        {
            var valueToDecrypt = new EncryptedValue(originalValue.IV.Value, originalValue.Value.Value);
            var decryptedValue = cryptoHandler.Decrypt(valueToDecrypt);
            JObject valueToReplace = originalValue;
            valueToReplace.Replace(decryptedValue);
        }

        private static void EncryptJsonValue(JsonPasswordCrypto cryptoHandler, JValue valueToEncrypt)
        {
            var value = valueToEncrypt.Value<string>();
            var encryptedValue = cryptoHandler.Encrypt(value);
            valueToEncrypt.Replace(JObject.FromObject(encryptedValue));
        }

        private static void EncryptTaggedValue(JsonPasswordCrypto cryptoHandler, dynamic valueToEncrypt)
        {
            var value = valueToEncrypt.encrypt.Value;
            var encryptedValue = cryptoHandler.Encrypt(value);
            valueToEncrypt.Replace(JObject.FromObject(encryptedValue));
        }
       
    }
}
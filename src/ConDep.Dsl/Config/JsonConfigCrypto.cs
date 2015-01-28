using System.Linq;
using ConDep.Dsl.Security;
using Newtonsoft.Json.Linq;

namespace ConDep.Dsl.Config
{
    public class JsonConfigCrypto : IHandleConfigCrypto<JObject>
    {
        private readonly JsonPasswordCrypto _valueHandler;

        public JsonConfigCrypto(string key)
        {
            _valueHandler = new JsonPasswordCrypto(key);
        }

        public JObject Decrypt(JObject config)
        {
            config.FindEncryptedTokens()
                .ForEach(x => DecryptJsonValue(_valueHandler, x));

            return config;
        }

        public JObject Encrypt(JObject config)
        {
            config.FindTaggedTokens("encrypt")
                .ForEach(x => EncryptTaggedValue(_valueHandler, x));

            var passwordTokens = config.SelectTokens("$..Password").OfType<JValue>().ToList();
            foreach (var token in passwordTokens)
            {
                EncryptJsonValue(_valueHandler, token);
            }
            return config;
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
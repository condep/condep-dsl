using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConDep.Dsl.Config
{
    public class EnvConfigParser
    {
        private JsonSerializerSettings _jsonSettings;

        public void UpdateConfig(string filePath, dynamic config)
        {
            File.WriteAllText(filePath, ConvertToJsonText(config));
        }

        public string ConvertToJsonText(dynamic config)
        {
            return JsonConvert.SerializeObject(config, JsonSettings);
        }

        public bool Encrypted(string jsonConfig, out dynamic jsonModel)
        {
            jsonModel = JObject.Parse(jsonConfig);

            var ivTokens = ((JObject)jsonModel).FindEncryptedTokens();

            return ivTokens
                .Select(token => token.ToObject<EncryptedValue>())
                .Any(value => 
                    value != null && 
                    !string.IsNullOrWhiteSpace(value.IV) && 
                    !string.IsNullOrWhiteSpace(value.Value)
                 );
        }

        public string GetConDepConfigFile(string env, string directory = null)
        {
            var dir = string.IsNullOrWhiteSpace(directory) ? Directory.GetCurrentDirectory() : directory;
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException(string.Format("Tried to find ConDep config files in directory [{0}], but directory does not exist.", dir));

            var dirInfo = new DirectoryInfo(dir);
            var fileName = string.Format("{0}.env.json", env);
            var configFiles = dirInfo.GetFiles(fileName);

            if (!configFiles.Any())
                throw new FileNotFoundException(string.Format("No ConDep configuration file found in directory [{0}] with name {1}", dir, fileName));

            return configFiles.Single().FullName;
        }

        public IEnumerable<string> GetConDepConfigFiles(string directory = null)
        {
            var dir = string.IsNullOrWhiteSpace(directory) ? Directory.GetCurrentDirectory() : directory;
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException(string.Format("Tried to find ConDep config files in directory [{0}], but directory does not exist.", dir));

            var dirInfo = new DirectoryInfo(dir);
            var configFiles = dirInfo.GetFiles("*.env.json");

            if (!configFiles.Any())
                throw new FileNotFoundException(string.Format("No ConDep configuration files found in directory [{0}]", dir));

            return configFiles.Select(x => x.FullName);
        }


        public ConDepEnvConfig GetTypedEnvConfig(string filePath, string cryptoKey)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("[{0}] not found.", filePath), filePath);
            }

            using (var fileStream = File.OpenRead(filePath))
            {
                return GetTypedEnvConfig(fileStream, cryptoKey);
            }
        }

        public string GetConDepConfigAsJsonText(string filePath)
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

        private JsonSerializerSettings JsonSettings
        {
            get
            {
                return _jsonSettings ?? (_jsonSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented,
                    });
            }
        }

        public void DecryptFile(string file, JsonPasswordCrypto crypto)
        {
            var json = GetConDepConfigAsJsonText(file);
            dynamic config;

            if (!Encrypted(json, out config))
            {
                throw new ConDepCryptoException("Unable to decrypt. No content in file [{0}] is encrypted.");                
            }

            DecryptJsonConfig(config, crypto);
            UpdateConfig(file, config);
        }

        public void DecryptJsonConfig(dynamic config, JsonPasswordCrypto crypto)
        {
            ((JObject)config).FindEncryptedTokens()
                .ForEach(x => DecryptJsonValue(crypto, x));
        }

        private void DecryptJsonValue(JsonPasswordCrypto crypto, dynamic originalValue)
        {
            var valueToDecrypt = new EncryptedValue(originalValue.IV.Value, originalValue.Value.Value);
            var decryptedValue = crypto.Decrypt(valueToDecrypt);
            JObject valueToReplace = originalValue;
            valueToReplace.Replace(decryptedValue);
        }

        public void EncryptFile(string file, JsonPasswordCrypto crypto)
        {
            var json = GetConDepConfigAsJsonText(file);
            dynamic config;

            if (Encrypted(json, out config))
                throw new ConDepCryptoException(string.Format("File [{0}] already encrypted.", file));

            EncryptJsonConfig(config, crypto);
            UpdateConfig(file, config);
        }

        public void EncryptJsonConfig(dynamic config, JsonPasswordCrypto crypto)
        {
            ((JObject) config).FindTaggedTokens("encrypt")
                .ForEach(x => EncryptTaggedValue(crypto, x));

            var passwordTokens = ((JObject) config).SelectTokens("$..Password").OfType<JValue>().ToList();
            foreach (var token in passwordTokens)
            {
                EncryptJsonValue(crypto, token);
            }
        }

        private static void EncryptJsonValue(JsonPasswordCrypto crypto, JValue valueToEncrypt)
        {
            var value = valueToEncrypt.Value<string>();
            var encryptedValue = crypto.Encrypt(value);
            valueToEncrypt.Replace(JObject.FromObject(encryptedValue));
        }

        private static void EncryptTaggedValue(JsonPasswordCrypto crypto, dynamic valueToEncrypt)
        {
            var value = valueToEncrypt.encrypt.Value;
            var encryptedValue = crypto.Encrypt("", value);
            valueToEncrypt.Replace(JObject.FromObject(encryptedValue));
        }

        public ConDepEnvConfig GetTypedEnvConfig(Stream stream, string cryptoKey)
        {
            ConDepEnvConfig config;
            using (var memStream = GetMemoryStreamWithCorrectEncoding(stream))
            {
                using (var reader = new StreamReader(memStream))
                {
                    var json = reader.ReadToEnd();
                    dynamic jsonModel;
                    if (Encrypted(json, out jsonModel))
                    {
                        if (string.IsNullOrWhiteSpace(cryptoKey))
                        {
                            throw new ConDepCryptoException(
                                "ConDep configuration is encrypted, so a decryption key is needed. Specify using -k switch.");
                        }
                        var crypto = new JsonPasswordCrypto(cryptoKey);
                        DecryptJsonConfig(jsonModel, crypto);
                        config = ((JObject) jsonModel).ToObject<ConDepEnvConfig>();
                    }
                    else
                    {
                        config = JsonConvert.DeserializeObject<ConDepEnvConfig>(json, JsonSettings);
                    }
                }
            }

            if (config.Servers != null && config.Tiers != null)
                throw new ConDepConfigurationException(
                    "You cannot define both Tiers and Servers at the same level. Either you use Tiers and define servers for each tier or you use Servers without Tiers. Servers without Tiers would be the same as having just one Tier."); 

            if(config.Servers == null) config.Servers = new List<ServerConfig>();

            if (config.Tiers == null)
            {
                foreach (var server in config.Servers.Where(server => !server.DeploymentUser.IsDefined()))
                {
                    server.DeploymentUser = config.DeploymentUser;
                }
            }
            else
            {
                foreach (var server in config.Tiers.SelectMany(tier => tier.Servers.Where(server => !server.DeploymentUser.IsDefined())))
                {
                    config.Servers.Add(server);
                    server.DeploymentUser = config.DeploymentUser;
                }
            }
            return config;

        }

        private static MemoryStream GetMemoryStreamWithCorrectEncoding(Stream stream)
        {
            using (var r = new StreamReader(stream, true))
            {
                var encoding = r.CurrentEncoding;
                return new MemoryStream(encoding.GetBytes(r.ReadToEnd()));
            }
        }
    }

    public static class JsonExtensions
    {
        public static List<JToken> FindTokens(this JToken containerToken, string name)
        {
            var matches = new List<JToken>();
            FindTokens(containerToken, name, matches);
            return matches;
        }

        public static List<JToken> FindTaggedTokens(this JToken containerToken, string tag)
        {
            var matches = new List<JToken>();
            FindTaggedTokens(containerToken, tag, matches);
            return matches;
        }

        public static List<JToken> FindEncryptedTokens(this JToken containerToken)
        {
            var matches = new List<JToken>();
            FindEncryptedTokens(containerToken, matches);
            return matches;
        }

        private static void FindTokens(JToken containerToken, string name, List<JToken> matches)
        {
            if (containerToken.Type == JTokenType.Object)
            {
                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    if (child.Name == name)
                    {
                        matches.Add(child.Value);
                    }
                    FindTokens(child.Value, name, matches);
                }
            }
            else if (containerToken.Type == JTokenType.Array)
            {
                foreach (JToken child in containerToken.Children())
                {
                    FindTokens(child, name, matches);
                }
            }
        }

        private static void FindTaggedTokens(JToken containerToken, string tag, List<JToken> matches)
        {
            if (containerToken.Type == JTokenType.Object)
            {
                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    if (child.Name == tag)
                    {
                        matches.Add(containerToken);
                    }
                    FindTaggedTokens(child.Value, tag, matches);
                }
            }
            else if (containerToken.Type == JTokenType.Array)
            {
                foreach (JToken child in containerToken.Children())
                {
                    FindTaggedTokens(child, tag, matches);
                }
            }
        }

        private static void FindEncryptedTokens(JToken containerToken, List<JToken> matches)
        {
            if (containerToken.Type == JTokenType.Object)
            {
                var children = containerToken.Children<JProperty>();
                if (children.Count() == 2)
                {
                    if(children.First().Name == "IV" && children.Last().Name == "Value")
                    {
                        matches.Add(containerToken);
                    }
                    else if (children.First().Name == "IV" && children.Last().Name == "Password")
                    {
                        throw new ConDepCryptoException(@"
Looks like you have an older environment encryption from an earlier version of ConDep. To correct this please replace ""Password"" key with ""Value"" in your Environment file(s). Example : 
    ""IV"": ""SaHK0yzgwDSAtE/oOhW0qg=="",
    ""Password"": ""Dcyn8fXnGnIG5rUw0BufzA==""

    replace ""Password"" key with ""Value"" like this:

    ""IV"": ""SaHK0yzgwDSAtE/oOhW0qg=="",
    ""Value"": ""Dcyn8fXnGnIG5rUw0BufzA==""
");
                    }
                    else
                    {
                        foreach (JProperty child in children)
                        {
                            FindEncryptedTokens(child.Value, matches);
                        }
                    }
                }
                else
                {
                    foreach (JProperty child in containerToken.Children<JProperty>())
                    {
                        FindEncryptedTokens(child.Value, matches);
                    }
                }

            }
            else if (containerToken.Type == JTokenType.Array)
            {
                foreach (JToken child in containerToken.Children())
                {
                    FindEncryptedTokens(child, matches);
                }
            }
        }
    }
}
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

            if (config.Node.Port == null) config.Node.Port = 4444;
            if (config.Node.TimeoutInSeconds == null) config.Node.TimeoutInSeconds = 100;
            
            if (config.PowerShell.HttpPort == null) config.PowerShell.HttpPort = 5985;
            if (config.PowerShell.HttpsPort == null) config.PowerShell.HttpsPort = 5986;

            foreach (var server in config.Servers)
            {
                if (server.Node == null)
                {
                    server.Node = config.Node;
                }

                if (server.PowerShell == null)
                {
                    server.PowerShell = config.PowerShell;
                }

                if (server.Node.Port == null) server.Node.Port = config.Node.Port;
                if (server.Node.TimeoutInSeconds == null) server.Node.TimeoutInSeconds = config.Node.TimeoutInSeconds;

                if (server.PowerShell.HttpPort == null) server.PowerShell.HttpPort = config.PowerShell.HttpPort;
                if (server.PowerShell.HttpsPort == null) server.PowerShell.HttpsPort = config.PowerShell.HttpsPort;
            }
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
}
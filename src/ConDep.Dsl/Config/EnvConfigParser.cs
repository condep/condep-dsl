using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConDep.Dsl.Config
{
    public class EnvConfigParser
    {
        private readonly ISerializerConDepConfig _configSerializer;

        public EnvConfigParser(ISerializerConDepConfig configSerializer)
        {
            _configSerializer = configSerializer;
        }

        public void UpdateConfig(string filePath, dynamic config)
        {
            File.WriteAllText(filePath, _configSerializer.Serialize(config));
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

        public ConDepEnvConfig GetTypedEnvConfig(Stream stream, string cryptoKey)
        {
            ConDepEnvConfig config = _configSerializer.DeSerialize(stream);

            //if (Encrypted(json, out jsonModel))
            //{
            //    if (string.IsNullOrWhiteSpace(cryptoKey))
            //    {
            //        throw new ConDepCryptoException(
            //            "ConDep configuration is encrypted, so a decryption key is needed. Specify using -k switch.");
            //    }
            //    var crypto = new JsonPasswordCrypto(cryptoKey);
            //    DecryptJsonConfig(jsonModel, crypto);
            //    config = ((JObject)jsonModel).ToObject<ConDepEnvConfig>();
            //}
            //else
            //{
            //    config = JsonConvert.DeserializeObject<ConDepEnvConfig>(json, JsonSettings);
            //}

            if (config.Servers != null && config.Tiers != null)
                throw new ConDepConfigurationException(
                    "You cannot define both Tiers and Servers at the same level. Either you use Tiers and define servers for each tier or you use Servers without Tiers. Servers without Tiers would be the same as having just one Tier."); 

            if(config.Servers == null) config.Servers = new List<IServerConfig>();

            if (config.Node.Port == null) config.Node.Port = 4444;
            if (config.Node.TimeoutInSeconds == null) config.Node.TimeoutInSeconds = 100;
            
            if (config.PowerShell.HttpPort == null) config.PowerShell.HttpPort = 5985;
            if (config.PowerShell.HttpsPort == null) config.PowerShell.HttpsPort = 5986;

            foreach (var server in config.UsingTiers ? config.Tiers.SelectMany(x => x.Servers) : config.Servers)
            {
                if (server.Node == null)
                {
                    server.Node = config.Node;
                }

                if (server.PowerShell == null)
                {
                    server.PowerShell = config.PowerShell;
                }

                if(!server.DeploymentUser.IsDefined()) server.DeploymentUser = config.DeploymentUser;

                if (server.Node.Port == null) server.Node.Port = config.Node.Port;
                if (server.Node.TimeoutInSeconds == null) server.Node.TimeoutInSeconds = config.Node.TimeoutInSeconds;

                if (server.PowerShell.HttpPort == null) server.PowerShell.HttpPort = config.PowerShell.HttpPort;
                if (server.PowerShell.HttpsPort == null) server.PowerShell.HttpsPort = config.PowerShell.HttpsPort;
            }
            return config;
        }
    }
}
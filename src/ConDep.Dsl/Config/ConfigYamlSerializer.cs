using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using YamlDotNet.Serialization;

namespace ConDep.Dsl.Config
{
    public class ConfigYamlSerializer : ISerializerConDepConfig
    {
        private readonly IHandleConfigCrypto _crypto;

        public ConfigYamlSerializer(IHandleConfigCrypto crypto)
        {
            _crypto = crypto;
        }

        public string Serialize(ConDepEnvConfig config)
        {
            using (var stringWriter = new StringWriter())
            {
                var serializer = new Serializer();
                serializer.Serialize(stringWriter, config);
                return stringWriter.ToString();
            }
        }

        public ConDepEnvConfig DeSerialize(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var deserialize = new Deserializer(ignoreUnmatched: true);
                deserialize.RegisterTagMapping("tag:yaml.org,2002:encrypt", typeof(string));
                return deserialize.Deserialize<ConDepEnvConfig>(reader);
            }
        }

        public ConDepEnvConfig DeSerialize(string config)
        {
            using (var stringReader = new StringReader(config))
            {
                var deserialize = new Deserializer(ignoreUnmatched: true);
                deserialize.RegisterTagMapping("tag:yaml.org,2002:encrypt", typeof(string));
                return deserialize.Deserialize<ConDepEnvConfig>(stringReader);
            }
        }
    }

    public class YamlEncrypt
    {
        private readonly string _secret;

        public YamlEncrypt(string secret)
        {
            _secret = secret;
        }

        public override string ToString()
        {
            return _secret;
        }
    }
}
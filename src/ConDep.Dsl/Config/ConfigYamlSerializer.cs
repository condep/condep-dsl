using System.IO;
using YamlDotNet.Serialization;

namespace ConDep.Dsl.Config
{
    public class ConfigYamlSerializer : ISerializerConDepConfig
    {
        public string Serialize(dynamic config)
        {
            throw new System.NotImplementedException();
        }

        public ConDepEnvConfig DeSerialize(Stream stream)
        {
            var reader = new StreamReader(stream);
            var deserialize = new Deserializer(ignoreUnmatched: true);
            return deserialize.Deserialize<ConDepEnvConfig>(reader);
        }

        public ConDepEnvConfig DeSerialize(string json)
        {
            throw new System.NotImplementedException();
        }
    }
}
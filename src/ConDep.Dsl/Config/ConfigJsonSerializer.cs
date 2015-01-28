using System.IO;
using ConDep.Dsl.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConDep.Dsl.Config
{
    public class ConfigJsonSerializer : ISerializerConDepConfig
    {
        private readonly IHandleConfigCrypto<JObject> _crypto;
        private JsonSerializerSettings _jsonSettings;

        public ConfigJsonSerializer(IHandleConfigCrypto<JObject> crypto )
        {
            _crypto = crypto;
        }

        public string Serialize(dynamic config)
        {
            return JsonConvert.SerializeObject(config, JsonSettings);
        }

        public ConDepEnvConfig DeSerialize(Stream stream)
        {
            ConDepEnvConfig config;
            using (var memStream = GetMemoryStreamWithCorrectEncoding(stream))
            {
                using (var reader = new StreamReader(memStream))
                {
                    var json = reader.ReadToEnd();
                    config = DeSerialize(json);
                }
            }
            return config;
        }

        public ConDepEnvConfig DeSerialize(string json)
        {
            if (_crypto.IsEncrypted(json))
            {
                var jsonModel = _crypto.Decrypt(JObject.Parse(json));
                return jsonModel.ToObject<ConDepEnvConfig>();
            }
            return JsonConvert.DeserializeObject<ConDepEnvConfig>(json, JsonSettings);
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
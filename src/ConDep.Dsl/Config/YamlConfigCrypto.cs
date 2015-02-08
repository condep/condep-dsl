using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ConDep.Dsl.Security;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Serialization.ObjectGraphVisitors;

namespace ConDep.Dsl.Config
{
    public class YamlConfigCrypto : IHandleConfigCrypto
    {
        private readonly JsonPasswordCrypto _valueHandler;

        public YamlConfigCrypto(string key)
        {
            _valueHandler = new JsonPasswordCrypto(key);
        }

        public string Decrypt(string config)
        {
            using (var reader = new StringReader(config))
            {
                var stream = new YamlStream();
                stream.Load(reader);
                var mapping = (YamlMappingNode)stream.Documents[0].RootNode;

                mapping.FindEncryptedNodes()
                    .ForEach(x => DecryptYamlValue(_valueHandler, x));


                var stringBuilder = new StringBuilder();
                using (var writer = new StringWriter(stringBuilder))
                {
                    stream.Save(writer);

                }
                return stringBuilder.ToString();
            }
        }

        public void DecryptFile(string filePath)
        {
            string yaml = GetConDepConfigAsYamlText(filePath);

            if (!IsEncrypted(yaml))
            {
                throw new ConDepCryptoException("Unable to decrypt. No content in file [{0}] is encrypted.");
            }

            var decryptedYaml = Decrypt(yaml);
            UpdateConfig(filePath, decryptedYaml);
        }

        private void UpdateConfig(string filePath, string yaml)
        {
            File.WriteAllText(filePath, yaml);
        }

        private string GetConDepConfigAsYamlText(string filePath)
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

        private static MemoryStream GetMemoryStreamWithCorrectEncoding(Stream stream)
        {
            using (var r = new StreamReader(stream, true))
            {
                var encoding = r.CurrentEncoding;
                return new MemoryStream(encoding.GetBytes(r.ReadToEnd()));
            }
        }

        public string Encrypt(string config)
        {
            var stream = new YamlStream();
            using (var reader = new StringReader(config))
            {
                stream.Load(reader);
            }
            
            var mapping = (YamlMappingNode)stream.Documents[0].RootNode;
            mapping.FindNodes("Password").ForEach(x => EncryptTaggedValue(_valueHandler, x));
            mapping.FindNodesToEncrypt().ForEach(x => EncryptTaggedValue(_valueHandler, x));

            var stringBuilder = new StringBuilder();
            using (var textWriter = new StringWriter(stringBuilder))
            {
                stream.Save(textWriter);
                return stringBuilder.ToString();
            }
        }

        public void EncryptFile(string filePath)
        {
            var yaml = GetConDepConfigAsYamlText(filePath);
            var encryptedYaml = Encrypt(yaml);
            UpdateConfig(filePath, encryptedYaml);
        }

        private static void DecryptYamlValue(JsonPasswordCrypto cryptoHandler, YamlEncryptedNode encryptedNode)
        {
            var decryptedValue = cryptoHandler.Decrypt(encryptedNode.EncryptedValue);
            encryptedNode.Parent.Children.Remove(encryptedNode.Container);
            encryptedNode.Parent.Add(encryptedNode.Container.Key, decryptedValue);
        }

        private static void EncryptTaggedValue(JsonPasswordCrypto cryptoHandler, KeyValuePair<YamlMappingNode, YamlNode> containerKeyPair)
        {
            var container = containerKeyPair.Key;
            var key = containerKeyPair.Value;
            var value = container.Children[key].ToString();
            var encryptedValue = cryptoHandler.Encrypt(value);

            container.Children.Remove(key);

            var serializer = new Serializer();

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, encryptedValue);

                var stream = new YamlStream();
                stream.Load(new StringReader(writer.ToString()));
                stream.Documents[0].RootNode.Tag = "tag:yaml.org,2002:encrypted";
                container.Add(key, stream.Documents[0].RootNode);
            }
        }

        public bool IsEncrypted(string config)
        {
            var stream = new YamlStream();
            using (var reader = new StringReader(config))
            {
                stream.Load(reader);

                return ((YamlMappingNode)stream.Documents[0].RootNode).FindEncryptedNodes().Any();
            }
        }
    }
}
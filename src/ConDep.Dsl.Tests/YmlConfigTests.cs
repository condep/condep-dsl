using System.IO;
using ConDep.Dsl.Config;
using ConDep.Dsl.Security;
using NUnit.Framework;
using YamlDotNet.RepresentationModel;

namespace ConDep.Dsl.Tests
{
    [TestFixture]
    public class YmlConfigTests
    {
        private string _yml = @"LoadBalancer:
  Name : jat-nlb01
  Provider : ConDep.Dsl.LoadBalancer.Ace.dll
  UserName : torresdal\\nlbUser
  Password : verySecureP@ssw0rd
  Mode : Sticky
  SuspendMode : Graceful
  CustomConfig :
    - Key : AwsSuspendWaitTime
      Value : 30
    - Key : AwsActivateWaitTime
      Value : 40

PowerShellScriptFolders : 
- psScripts
- psScripts\\subScripts
- psScripts\\subScripts\\subSubScripts

Servers :
- Name : jat-web01
  LoadBalancerFarm : farm1
  StopServer: true
  WebSites : 
    - Name : WebSite1 
      Bindings : 
      - BindingType : http
        Port : 80
        Ip : 10.0.0.111
        HostHeader : 

      - BindingType : https
        Port : 443
        Ip : 10.0.0.111
        HostHeader : 

    - Name : WebSite2
      Bindings : 
      - BindingType : http
        Port : 80
        Ip : 10.0.0.112
        HostHeader : 

      - BindingType : https
        Port : 443
        Ip : 10.0.0.112
        HostHeader : 

    - Name : WebSite3
      Bindings :
        - BindingType : http
          Port : 80
          Ip : 10.0.0.113
          HostHeader :

        - BindingType : https
          Port : 443
          Ip : 10.0.0.113
          HostHeader : 

- Name : jat-web02
  LoadBalancerFarm : farm1
  DeploymentUser :
    UserName : torresdal\\asdfasdf
    Password : lakjlskjdfkwe
  WebSites : 
    - Name : WebSite1
      Bindings :
        - BindingType : http
          Port : 80
          Ip : 10.0.0.121
          HostHeader : 

        - BindingType : https
          Port : 443
          Ip : 10.0.0.121
          HostHeader : 

    - Name : WebSite2
      Bindings : 
        - BindingType : http
          Port : 80
          Ip : 10.0.0.122
          HostHeader : 
        - BindingType : https
          Port : 443
          Ip : 10.0.0.122
          HostHeader : 

    - Name : WebSite3
      Bindings :
        - BindingType : http
          Port : 80
          Ip : 10.0.0.123
          HostHeader : 
        - BindingType : https
          Port : 443
          Ip : 10.0.0.123
          HostHeader : 
DeploymentUser : 
  UserName : torresdal\\condepuser
  Password : verySecureP@ssw0rd

OperationsConfig :
  NServiceBusOperation : 
    ServiceUserName : torresdal\\nservicebususer
    ServicePassword : !!encrypt verySecureP@ssw0rd

  SomeOtherOperation :
    SomeOtherSetting1 : asdfasdf
    SomeOtherSetting2 : 34tsdfg";

        [Test]
        public void TestThat_YamlCanBeLoadedIntoModel()
        {
            var key = JsonPasswordCrypto.GenerateKey(256);
            var serializer = new ConfigYamlSerializer(new YamlConfigCrypto(key));

            var condepConfig = serializer.DeSerialize(_yml);
        }

        [Test]
        public void TestThat_CanEncryptYaml()
        {
            var key = JsonPasswordCrypto.GenerateKey(256);
            var crypto = new YamlConfigCrypto(key);

            var encryptedYml = crypto.Encrypt(_yml);
            Assert.That(crypto.IsEncrypted(encryptedYml));
        }

        [Test]
        public void TestThat_CanDecryptYaml()
        {
            var key = JsonPasswordCrypto.GenerateKey(256);
            var crypto = new YamlConfigCrypto(key);

            var encryptedYml = crypto.Encrypt(_yml);
            Assert.That(crypto.IsEncrypted(encryptedYml));

            var decryptedYml = crypto.Decrypt(encryptedYml);
            Assert.That(!crypto.IsEncrypted(decryptedYml));
        }

        [Test]
        public void TestThat_CanEncryptPassword()
        {
            string yml = @"UserName : Administrator
Password : !!encrypt SomePassword";

            var key = JsonPasswordCrypto.GenerateKey(256);
            var crypto = new YamlConfigCrypto(key);

            var encryptedYml = crypto.Encrypt(yml);
            Assert.That(crypto.IsEncrypted(encryptedYml));
        }

        [Test]
        public void TestThat_CanDecryptPassword()
        {
            var key = "RYqIgFuJQA5E9LeKRY+F7uJKD7qjs97jcsJ0IYrOAOs=";
            string yml = @"UserName: Administrator
Password:
    IV: BYmlG/ynplh5JZ3mZTbfaQ==
    Value: QAjlFzJPk1q5+qnqaEybpQ==";

            var crypto = new YamlConfigCrypto(key);
            Assert.That(crypto.IsEncrypted(yml));

            var decryptedYml = crypto.Decrypt(yml);
            Assert.That(!crypto.IsEncrypted(decryptedYml));

            using (var reader = new StringReader(decryptedYml))
            {
                var stream = new YamlStream();
                stream.Load(reader);

                var node = ((YamlMappingNode) stream.Documents[0].RootNode).Children[new YamlScalarNode("Password")] as YamlScalarNode;
                Assert.That(node.Value, Is.EqualTo("SomePassword"));
            }
        }

        [Test]
        public void TestThat_CanEncryptTaggedForEncryption()
        {
            string yml = @"SomeKey : Administrator
SomeSensitiveKey : !!encrypt SomeSensitiveValue";

            var key = JsonPasswordCrypto.GenerateKey(256);
            var crypto = new YamlConfigCrypto(key);

            var encryptedYml = crypto.Encrypt(yml);
            Assert.That(crypto.IsEncrypted(encryptedYml));
        }

        [Test]
        public void TestThat_CanDecryptTaggedForEncryption()
        {
            var key = "fbxgmcR6eyxA1DIDHbFj2H3HNWopsjYL1hhx3DUALAk=";
            string yml = @"SomeKey: Administrator
SomeSensitiveKey:
    IV: QQm4IgHsOVLyoA0izfzfGw==
    Value: tBkEGpVzcNpommpdPWVa8X9QQJkaTnFSW0Q5yGJicL8=";

            var crypto = new YamlConfigCrypto(key);
            Assert.That(crypto.IsEncrypted(yml));

            var decryptedYml = crypto.Decrypt(yml);
            Assert.That(!crypto.IsEncrypted(decryptedYml));

        }
    }
}
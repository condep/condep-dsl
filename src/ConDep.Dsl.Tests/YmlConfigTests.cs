using System.IO;
using ConDep.Dsl.Config;
using NUnit.Framework;
using YamlDotNet.Dynamic;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

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

  CustomValues :
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
    ServicePassword : verySecureP@ssw0rd

  SomeOtherOperation :
    SomeOtherSetting1 : asdfasdf
    SomeOtherSetting2 : 34tsdfg";

        [Test]
        public void TestThat_()
        {
            var reader = new StringReader(_yml);

            dynamic dynYml = new DynamicYaml(reader);
            var lb = dynYml.LoadBalancer;
        }

        [Test]
        public void TestThat_LoadYamlIntoModel()
        {
            var reader = new StringReader(_yml);
            var deserialize = new Deserializer(ignoreUnmatched: true);
            var config = deserialize.Deserialize<ConDepEnvConfig>(reader);
        }
    }
}
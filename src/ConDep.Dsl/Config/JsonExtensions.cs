using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Security;
using Newtonsoft.Json.Linq;

namespace ConDep.Dsl.Config
{
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
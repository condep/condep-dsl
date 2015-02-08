using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Security.Cryptography.X509Certificates;
using ConDep.Dsl.Security;
using YamlDotNet.RepresentationModel;

namespace ConDep.Dsl.Config
{
    public class YamlEncryptedNode
    {
        public YamlMappingNode Parent { get; set; }
        public KeyValuePair<YamlNode, YamlNode> Container { get; set; }

        public EncryptedValue EncryptedValue
        {
            get
            {
                return new EncryptedValue(((YamlMappingNode)Container.Value).Children[new YamlScalarNode("IV")].ToString(), ((YamlMappingNode)Container.Value).Children[new YamlScalarNode("Value")].ToString());
            }
        }
    }

    public static class YamlExtensions
    {
        private const string _encryptTag = "tag:yaml.org,2002:encrypt";
        private const string _encryptedTag = "tag:yaml.org,2002:encrypted";

        public static List<KeyValuePair<YamlMappingNode, YamlNode>> FindNodes(this YamlMappingNode container, string name)
        {
            var matches = new List<KeyValuePair<YamlMappingNode, YamlNode>>();
            FindNodes(container, name, matches);
            return matches;
        }

        public static List<KeyValuePair<YamlMappingNode, YamlNode>> FindNodesToEncrypt(this YamlMappingNode container)
        {
            var matches = new List<KeyValuePair<YamlMappingNode, YamlNode>>();
            FindNodesToEncrypt(container, matches);
            return matches;
        }

        private static void FindNodesToEncrypt(YamlMappingNode container, List<KeyValuePair<YamlMappingNode, YamlNode>> matches)
        {
            foreach (var node in container.Children)
            {
                if (node.Value.Tag != null && node.Value.Tag.Equals(_encryptTag, StringComparison.InvariantCultureIgnoreCase))
                {
                    matches.Add(new KeyValuePair<YamlMappingNode, YamlNode>(container, node.Key));
                }
            }

            var nodes = container.Children.Values.OfType<YamlMappingNode>();
            foreach (var node in nodes)
            {
                FindNodesToEncrypt(node, matches);
            }

            var sequenceNodes = container.Children.Values.OfType<YamlSequenceNode>();
            foreach (var sequenceNode in sequenceNodes)
            {
                foreach (var node in sequenceNode.Children.OfType<YamlMappingNode>())
                {
                    FindNodesToEncrypt(node, matches);
                }
            }
        }

        public static List<YamlEncryptedNode> FindEncryptedNodes(this YamlMappingNode container)
        {
            var matches = new List<YamlEncryptedNode>();
            container.Children.ToList().ForEach(x => FindEncryptedNodes(container, x, matches));
            return matches;
        }

        private static void FindEncryptedNodes(YamlMappingNode parent, KeyValuePair<YamlNode, YamlNode> container, List<YamlEncryptedNode> matches)
        {
            var node = container.Value;
            if (node is YamlMappingNode)
            {
                var nodes = ((YamlMappingNode) node).Children.ToList();

                if (nodes.Count == 2 &&
                    nodes[0].Key.ToString().Equals("IV", StringComparison.InvariantCultureIgnoreCase) &&
                    nodes[1].Key.ToString().Equals("Value", StringComparison.InvariantCultureIgnoreCase))
                {
                    var encryptedNode = new YamlEncryptedNode {Container = container, Parent = parent};
                    matches.Add(encryptedNode);
                }
            }

            if (node is YamlMappingNode)
            {
                foreach (var childNode in ((YamlMappingNode)node).Children)
                {
                    FindEncryptedNodes((YamlMappingNode) node, childNode, matches);
                }
            }

            if (node is YamlSequenceNode)
            {
                var sequenceNodes = ((YamlSequenceNode) node).Children;
                foreach (var child in sequenceNodes.OfType<YamlMappingNode>())
                {
                    child.Children.ToList().ForEach(x => FindEncryptedNodes(child, x, matches));
                }
            }
        }

        private static void FindNodes(YamlMappingNode container, string name, List<KeyValuePair<YamlMappingNode, YamlNode>> matches)
        {
            var key = new YamlScalarNode(name);
            YamlNode matchNode;

            if (container.Children.TryGetValue(key, out matchNode))
            {
                matches.Add(new KeyValuePair<YamlMappingNode, YamlNode>(container, new YamlScalarNode(name)));
            }

            var nodes = container.Children.Values.OfType<YamlMappingNode>();
            foreach (var node in nodes)
            {
                FindNodes(node, name, matches);
            }

            var sequenceNodes = container.Children.Values.OfType<YamlSequenceNode>();
            foreach (var sequenceNode in sequenceNodes)
            {
                foreach (var node in sequenceNode.Children.OfType<YamlMappingNode>())
                {
                    FindNodes(node, name, matches);
                }
            }
        }
    }
}
using System.Linq;
using System.Text.RegularExpressions;

namespace ConDep.Dsl.PSScripts.ConDepNode
{
    public class ConDepNodeResources
    {
        public static string ConDepNodeModule
        {
            get
            {
                var type = typeof(ConDepNodeResources);
                var resources = type.Assembly.GetManifestResourceNames();
                var pattern = type.Namespace + @"\..+\.psm1";
                var regex = new Regex(pattern);
                return resources.Single(psPath => regex.Match(psPath).Success);
            }
        }
    }
}
using System.Linq;
using System.Text.RegularExpressions;

namespace ConDep.Dsl.PSScripts.ConDep
{
    public class ConDepResources
    {
        public static string ConDepModule
        {
            get
            {
                var type = typeof(ConDepResources);
                var resources = type.Assembly.GetManifestResourceNames();
                var pattern = type.Namespace + @"\..+\.psm1";
                var regex = new Regex(pattern);
                return resources.Single(psPath => regex.Match(psPath).Success);
            }
        }
    }
}
namespace ConDep.Dsl.PSScripts.PfxInstaller
{
    public class PfxInstallerResource
    {
        public static ConDepResource PfxInstallerScript
        {
            get
            {
                var resourceName = "PfxInstaller.script";
                var type = typeof(PfxInstallerResource);

                return new ConDepResource
                {
                    Resource = resourceName,
                    Namespace = type.Namespace
                };
            }
        }
 
    }

    public class ConDepResource
    {
        public string Resource { get; set; }
        public string Namespace { get; set; }
    }
}
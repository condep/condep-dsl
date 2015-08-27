namespace ConDep.Dsl.Remote
{
    public class PowerShellModulesToLoad
    {
        public PowerShellModulesToLoad()
        {
            LoadConDepModule = true;
        }

        public bool LoadConDepModule { get; set; }
        public bool LoadConDepNodeModule { get; set; }
        public bool LoadConDepDotNetLibrary { get; set; }
    }
}
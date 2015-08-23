using ConDep.Dsl.Config;

namespace ConDep.Dsl.Remote
{
    public interface IPublishFiles
    {
        void PublishFile(string srcFile, string dstFile, IServerConfig server, ConDepSettings settings);
        void PublishDirectory(string srcDir, string dstDir, IServerConfig server, ConDepSettings settings);
    }
}
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Remote
{
    public interface IPublishFiles
    {
        void PublishFile(string srcFile, string dstFile, ServerConfig server, ConDepSettings settings);
        void PublishDirectory(string srcDir, string dstDir, ServerConfig server, ConDepSettings settings);
    }
}
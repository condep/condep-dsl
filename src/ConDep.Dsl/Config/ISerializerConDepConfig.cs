using System.IO;

namespace ConDep.Dsl.Config
{
    public interface ISerializerConDepConfig
    {
        string Serialize(ConDepEnvConfig config);
        ConDepEnvConfig DeSerialize(Stream stream);
        ConDepEnvConfig DeSerialize(string config);
    }
}
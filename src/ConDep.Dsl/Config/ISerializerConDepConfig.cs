using System.IO;

namespace ConDep.Dsl.Config
{
    public interface ISerializerConDepConfig
    {
        string Serialize(dynamic config);
        ConDepEnvConfig DeSerialize(Stream stream);
        ConDepEnvConfig DeSerialize(string json);
    }
}
using System.IO;

namespace ConDep.Dsl.Config
{
    public class ConDepFileReader 
    {
        public static string ReadText(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("[{0}] not found.", filePath), filePath);
            }

            using (var fileStream = File.OpenRead(filePath))
            {
                using (var memStream = GetMemoryStreamWithCorrectEncoding(fileStream))
                {
                    using (var reader = new StreamReader(memStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        private static MemoryStream GetMemoryStreamWithCorrectEncoding(Stream stream)
        {
            using (var r = new StreamReader(stream, true))
            {
                var encoding = r.CurrentEncoding;
                return new MemoryStream(encoding.GetBytes(r.ReadToEnd()));
            }
        }
    }
}
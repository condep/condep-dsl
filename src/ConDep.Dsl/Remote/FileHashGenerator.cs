using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ConDep.Dsl.Remote
{
    internal class FileHashGenerator
    {
        public static IEnumerable<Tuple<string, string>> GetFileHash(IEnumerable<string> filePaths)
        {
            var scriptFilesList = filePaths.ToList();
            return scriptFilesList.Select(file => new Tuple<string, string>(Path.GetFileName(file), GetFileHash(file)));
        }

        public static string GetFileHash(string file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    var byteHash = md5.ComputeHash(stream);
                    return String.Concat(Array.ConvertAll(byteHash, x => x.ToString("X2")));
                }
            }
        }
    }
}
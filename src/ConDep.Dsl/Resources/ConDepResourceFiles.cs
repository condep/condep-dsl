using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ConDep.Dsl.Resources
{
    public class ConDepResourceFiles
    {
        public static string ExtractPowerShellFileFromResource(Assembly assembly, string resource)
        {
            var regex = new Regex(@".+\.(.+\.(ps1|psm1))");
            var match = regex.Match(resource);
            if (match.Success)
            {
                var resourceName = match.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(resourceName))
                {
                    var resourceNamespace = resource.Replace("." + resourceName, "");
                    return GetFilePath(assembly, resourceNamespace, resourceName, keepOriginalFileName: true);
                }
            }
            return null;
        }

        public static string GetFilePath(Assembly assembly, string resourceNamespace, string resourceName, string dirPath = "", bool keepOriginalFileName = false)
        {
            //Todo: not thread safe
            var tempFolder = string.IsNullOrWhiteSpace(dirPath) ? Path.GetTempPath() : dirPath;
            var filePath = Path.Combine(tempFolder, resourceName + (keepOriginalFileName ? "" : ".condep"));

            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            try
            {
                using (var stream = assembly.GetManifestResourceStream(resourceNamespace + "." + resourceName))
                {
                    if(stream == null)
                    {
                        throw new ConDepResourceNotFoundException(string.Format("Unable to find resource [{0}]", resourceName));    
                    }

                    using (var writeStream = File.Create(filePath))
                    {
                        stream.CopyTo(writeStream);
                    }
                }
                return filePath;
            }
            catch (Exception ex)
            {
                throw new ConDepResourceNotFoundException(string.Format("Resource [{0}]", resourceName), ex);
            }
        }

        internal static string GetFilePath(string resourceNamespace, string resourceName, bool keepOriginalFileName = false)
        {
            return GetFilePath(Assembly.GetExecutingAssembly(), resourceNamespace, resourceName, keepOriginalFileName: keepOriginalFileName);
        }
    }
}
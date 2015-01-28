using System;
using System.Collections.Generic;
using System.IO;

namespace ConDep.Dsl.Config
{
    public static class ConfigHandler
    {
        public static List<string> SupportedFileExtensions = new List<string> { ".json", ".yml", ".yaml" };

        public static ConDepEnvConfig GetEnvConfig(ConDepSettings settings)
        {
            string envFilePath;
            var jsonConfigParser = new EnvConfigParser(ResolveConfigParser(settings, out envFilePath));

            var envConfig = jsonConfigParser.GetTypedEnvConfig(envFilePath, settings.Options.CryptoKey);
            envConfig.EnvironmentName = settings.Options.Environment;

            if (settings.Options.BypassLB)
            {
                envConfig.LoadBalancer = null;
            }
            return envConfig;
        }

        private static ISerializerConDepConfig ResolveConfigParser(ConDepSettings settings, out string filePath)
        {
            var searchDir = Path.GetDirectoryName(settings.Options.Assembly.Location);
            var searchPattern = string.Format("{0}.env.*", settings.Options.Environment);

            var configFiles = Directory.GetFiles(
                searchDir,
                searchPattern, 
                SearchOption.TopDirectoryOnly
            );

            if (configFiles.Length == 0)
                throw new FileNotFoundException(string.Format("No envrionment configuration files found in {0}", searchDir));

            if (configFiles.Length > 1)
                throw new FileNotFoundException(string.Format("More than one envrionment configuration file found in {0} matching the serarch pattern {1}", searchDir, searchPattern));

            var file = new FileInfo(configFiles[0]);
            if (!SupportedFileExtensions.Exists(x => x.Equals(file.Extension, StringComparison.InvariantCultureIgnoreCase)))
                throw new FileNotFoundException(string.Format("Envrionment configuration file with extension {0} is not currently supported. Currently supported config file extensions are {1}", file.Extension, string.Join(", ", SupportedFileExtensions)));

            filePath = file.FullName;

            switch (file.Extension.ToLower())
            {
                case ".json":
                    return new ConfigJsonSerializer(new JsonConfigCrypto(settings.Options.CryptoKey));
                case ".yml":
                case ".yaml":
                    return new ConfigYamlSerializer();
                default:
                    throw new FileNotFoundException();
            }
        }
    }
}
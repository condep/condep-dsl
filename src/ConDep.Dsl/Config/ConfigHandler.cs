using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConDep.Dsl.Config
{
    public static class ConfigHandler
    {
        public static List<string> SupportedFileExtensions = new List<string> { ".json", ".yml", ".yaml" };

        public static string GetConDepConfigFile(string env, string directory = null)
        {
            var dir = string.IsNullOrWhiteSpace(directory) ? Directory.GetCurrentDirectory() : directory;
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException(string.Format("Tried to find ConDep config files in directory [{0}], but directory does not exist.", dir));

            var dirInfo = new DirectoryInfo(dir);
            var fileName = string.Format("{0}.env.*", env);
            var configFiles = dirInfo.GetFiles(fileName, SearchOption.TopDirectoryOnly);
            configFiles = configFiles.Where(x => ConfigHandler.SupportedFileExtensions.Exists(ext => ext.Equals(x.Extension))).ToArray();

            if (!configFiles.Any())
                throw new FileNotFoundException(string.Format("No ConDep configuration file found in directory [{0}] with name {1}", dir, fileName));

            return configFiles.Single().FullName;
        }

        public static IEnumerable<string> GetConDepConfigFiles(string directory = null)
        {
            var dir = string.IsNullOrWhiteSpace(directory) ? Directory.GetCurrentDirectory() : directory;
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException(string.Format("Tried to find ConDep config files in directory [{0}], but directory does not exist.", dir));

            var dirInfo = new DirectoryInfo(dir);
            var configFiles = dirInfo.GetFiles("*.env.*", SearchOption.TopDirectoryOnly);
            configFiles = configFiles.Where(x => ConfigHandler.SupportedFileExtensions.Exists(ext => ext.Equals(x.Extension))).ToArray();

            if (!configFiles.Any())
                throw new FileNotFoundException(string.Format("No ConDep configuration files found in directory [{0}]", dir));

            return configFiles.Select(x => x.FullName);
        }

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

        public static ISerializerConDepConfig ResolveConfigParser(ConDepSettings settings, out string filePath)
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

            return ResolveConfigParser(filePath, settings.Options.CryptoKey);
        }

        public static ISerializerConDepConfig ResolveConfigParser(string filePath, string cryptoKey)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(string.Format("File [{0}] not found.", filePath));

            var fileInfo = new FileInfo(filePath);

            switch (fileInfo.Extension.ToLower())
            {
                case ".json":
                    return new ConfigJsonSerializer(new JsonConfigCrypto(cryptoKey));
                case ".yml":
                case ".yaml":
                    return new ConfigYamlSerializer(new JsonConfigCrypto(cryptoKey));
                default:
                    throw new FileNotFoundException();
            }
        }

        public static IHandleConfigCrypto ResolveConfigCrypto(string filePath, string cryptoKey)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(string.Format("File [{0}] not found.", filePath));

            var fileInfo = new FileInfo(filePath);

            switch (fileInfo.Extension.ToLower())
            {
                case ".json":
                    return new JsonConfigCrypto(cryptoKey);
                case ".yml":
                case ".yaml":
                    return new YamlConfigCrypto(cryptoKey);
                default:
                    throw new FileNotFoundException();
            }
        }
    }
}
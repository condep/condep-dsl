using ConDep.Dsl.Operations.Application.Local.PreCompile;
using ConDep.Dsl.Operations.Application.Local.TransformConfig;
using ConDep.Dsl.Operations.Application.Local.WebRequest;

namespace ConDep.Dsl
{
    public static class LocalOperationsExtensions
    {
        /// <summary>
        /// Transforms .NET configuration files (web and app config), in exactly the same way as msbuild and Visual Studio does.
        /// </summary>
        /// <param name="configDirPath"></param>
        /// <param name="configName"></param>
        /// <param name="transformName"></param>
        /// <returns></returns>
        public static IOfferLocalOperations TransformConfigFile(this IOfferLocalOperations local, string configDirPath, string configName, string transformName)
        {
            var operation = new TransformConfigOperation(configDirPath, configName, transformName);
            Configure.LocalOperations.AddOperation(operation);
            return local;
        }

        /// <summary>
        /// Pre-compile Web Applications to optimize startup time for the application. Even though this operation exist in ConDep, we recommend you to pre-compile web applications as part of your build process, and not the deployment process, using aspnet_compiler.exe.
        /// </summary>
        /// <param name="webApplicationName"></param>
        /// <param name="webApplicationPhysicalPath"></param>
        /// <param name="preCompileOutputpath"></param>
        /// <returns></returns>
        public static IOfferLocalOperations PreCompile(this IOfferLocalOperations local, string webApplicationName, string webApplicationPhysicalPath, string preCompileOutputpath)
        {
            var operation = new PreCompileOperation(webApplicationName, webApplicationPhysicalPath,
                                                                          preCompileOutputpath);
            Configure.LocalOperations.AddOperation(operation);
            return local;
        }

        /// <summary>
        /// Executes a simple HTTP GET to the specified url expecting a 200 (OK) in return. Will throw an exception if not 200.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IOfferLocalOperations HttpGet(this IOfferLocalOperations local, string url)
        {
            var operation = new HttpGetOperation(url);
            Configure.LocalOperations.AddOperation(operation);
            return local;
        }

    }
}
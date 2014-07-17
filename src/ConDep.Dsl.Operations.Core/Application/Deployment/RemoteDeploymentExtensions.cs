using System;
using ConDep.Dsl.Operations.Application.Deployment.CopyDir;
using ConDep.Dsl.Operations.Application.Deployment.CopyFile;
using ConDep.Dsl.Operations.Application.Deployment.WebApp;
using ConDep.Dsl.Operations.Application.Deployment.WindowsService;
using ConDep.Dsl.Operations.Application.Deployment.NServiceBus;

namespace ConDep.Dsl
{
    public static class RemoteDeploymentExtensions
    {
        /// <summary>
        /// Will deploy local source directory to remote destination directory. This operation does dot just copy directory content, but synchronize the the source folder to the destination. If a file already exist on destination, it will be updated to match source file. If a file exist on destination, but not in source directory, it will be removed from destination. If a file is readonly on destination, but read/write in source, destination file will be updated with read/write.
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        /// <returns></returns>
        public static IOfferRemoteDeployment Directory(this IOfferRemoteDeployment remote, string sourceDir, string destDir)
        {
            var copyDirOperation = new CopyDirOperation(sourceDir, destDir);
            Configure.DeploymentOperations.AddOperation(copyDirOperation);
            return remote;
        }

        /// <summary>
        /// Will deploy local file and its attributes to remote server.
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destFile"></param>
        /// <returns></returns>
        public static IOfferRemoteDeployment File(this IOfferRemoteDeployment remote, string sourceFile, string destFile)
        {
            var copyFileOperation = new CopyFileOperation(sourceFile, destFile);
            Configure.DeploymentOperations.AddOperation(copyFileOperation);
            return remote;
        }

        /// <summary>
        /// Works exactly as the Directory operation, except it will mark the directory as a Web Application on remote server.
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="webAppName"></param>
        /// <param name="webSiteName"></param>
        /// <returns></returns>
        public static IOfferRemoteDeployment IisWebApplication(this IOfferRemoteDeployment remote, string sourceDir, string webAppName, string webSiteName)
        {
            return IisWebApplication(remote, sourceDir, null, webAppName, webSiteName);
        }

        /// <summary>
        /// Works exactly as the Directory operation, except it will mark the directory as a Web Application on remote server.
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        /// <param name="webAppName"></param>
        /// <param name="webSiteName"></param>
        /// <returns></returns>
        public static IOfferRemoteDeployment IisWebApplication(this IOfferRemoteDeployment remote, string sourceDir, string destDir, string webAppName, string webSiteName)
        {
            var webAppOperation = new WebAppOperation(sourceDir, webAppName, webSiteName, destDir);
            Configure.DeploymentOperations.AddOperation(webAppOperation);
            return remote;
        }

        /// <summary>
        /// Will deploy and start provided Windows Service to remote server.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        /// <param name="relativeExePath"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public static IOfferRemoteDeployment WindowsService(this IOfferRemoteDeployment remote, string serviceName, string displayName, string sourceDir, string destDir, string relativeExePath)
        {
            return WindowsService(remote, serviceName, displayName, sourceDir, destDir, relativeExePath, null);
        }

        /// <summary>
        /// Will deploy and start provided Windows Service to remote server with provided options.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        /// <param name="relativeExePath"></param>
        /// <param name="displayName"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IOfferRemoteDeployment WindowsService(this IOfferRemoteDeployment remote, string serviceName, string displayName, string sourceDir, string destDir, string relativeExePath, Action<IOfferWindowsServiceOptions> options)
        {
            var winServiceOptions = new WindowsServiceOptions();
            if (options != null)
            {
                options(winServiceOptions);
            }

            var winServiceOperation = new WindowsServiceOperation(serviceName, displayName, sourceDir, destDir, relativeExePath, winServiceOptions.Values);
            Configure.DeploymentOperations.AddOperation(winServiceOperation);
            return remote;
        }

        /// <summary>
        /// Will deploy and start provided Windows Service using the built-in installer.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        /// <param name="relativeExePath"></param>
        /// <param name="displayName"></param>
        /// <param name="installerParams"></param>
        /// <returns></returns>
        public static IOfferRemoteDeployment WindowsServiceWithInstaller(this IOfferRemoteDeployment remote, string serviceName, string displayName, string sourceDir, string destDir, string relativeExePath, string installerParams)
        {
            return WindowsServiceWithInstaller(remote, serviceName, displayName, sourceDir, destDir, relativeExePath, installerParams, null);
        }

        /// <summary>
        /// Will deploy and start provided Windows Service with provided options using the built-in installer.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        /// <param name="relativeExePath"></param>
        /// <param name="displayName"></param>
        /// <param name="installerParams"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IOfferRemoteDeployment WindowsServiceWithInstaller(this IOfferRemoteDeployment remote, string serviceName, string displayName, string sourceDir, string destDir, string relativeExePath, string installerParams, Action<IOfferWindowsServiceOptions> options)
        {
            var winServiceOptions = new WindowsServiceOptions();
            if (options != null)
            {
                options(winServiceOptions);
            }

            var winServiceOperation = new WindowsServiceWithInstallerOperation(serviceName, displayName, sourceDir, destDir, relativeExePath, installerParams, winServiceOptions.Values);
            Configure.DeploymentOperations.AddOperation(winServiceOperation);
            return remote;
        }

        /// <summary>
        /// Exactly the same as the WindowsService operation, only tailored for NServiceBus.
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        /// <param name="serviceName"></param>
        /// <param name="profile"> </param>
        /// <returns></returns>
        public static IOfferRemoteDeployment NServiceBusEndpoint(this IOfferRemoteDeployment remote, string sourceDir, string destDir, string serviceName, string profile)
        {
            return NServiceBusEndpoint(remote, sourceDir, destDir, serviceName, profile, null);
        }

        /// <summary>
        /// Exactly the same as the WindowsService operation, only tailored for NServiceBus.
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        /// <param name="serviceName"></param>
        /// <param name="profile"> </param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IOfferRemoteDeployment NServiceBusEndpoint(this IOfferRemoteDeployment remote, string sourceDir, string destDir, string serviceName, string profile, Action<IOfferWindowsServiceOptions> options)
        {
            var nServiceBusProvider = new NServiceBusOperation(sourceDir, destDir, serviceName, profile, options);
            Configure.DeploymentOperations.AddOperation(nServiceBusProvider);
            return remote;
        }

        /// <summary>
        /// Provide operations for deploying SSL certificates to remote server.
        /// </summary>
        //public static IOfferRemoteCertDeployment SslCertificate { get { return new RemoteCertDeploymentBuilder(_remoteSequence, this); } }
    }
}
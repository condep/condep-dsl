using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Xml;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Remote;
using ConDep.Dsl.Validation;
using Newtonsoft.Json;

namespace ConDep.Dsl
{
    public abstract class RemoteServerOperation : IExecuteRemotely
    {
        private readonly IPublishFiles _filePublisher;

        protected RemoteServerOperation(IPublishFiles filePublisher)
        {
            _filePublisher = filePublisher;
        }

        protected RemoteServerOperation(params object[] constructorArguments)
        {
            ConstructorArguments = constructorArguments.ToList();
        }

        public List<object> ConstructorArguments { get; private set; } 

        public abstract void Execute(ILogForConDep logger);

        public void Execute(IServerConfig server, IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            var assemblyLocalDir = Path.GetDirectoryName(GetType().Assembly.Location);
            var assemblyRemoteDir = Path.Combine(server.GetServerInfo().TempFolderDos, "Assemblies");

            _filePublisher.PublishDirectory(assemblyLocalDir, assemblyRemoteDir, server, settings);

            var remoteAssemblyFileName = Path.Combine(Path.Combine(server.GetServerInfo().TempFolderPowerShell, "Assemblies"), Path.GetFileName(GetType().Assembly.Location));
            var remoteJsonAssembly = Path.Combine(Path.Combine(server.GetServerInfo().TempFolderPowerShell, "Assemblies"), "Newtonsoft.Json.dll");
            var typeName = GetType().FullName;
            var loggerTypeName = typeof (RemotePowerShellLogger).FullName;

            var parameters = GetPowerShellParameters(ConstructorArguments, GetType()).ToList();
            var scriptParams = string.Join(",", parameters.Select(x => "$" + x.Name));
            var argumentList = string.Join(",", GetType().GetConstructor(ConstructorArguments.Select(x => x.GetType()).ToArray()).GetParameters().Select(x => "$" + x.Name));
            var deserializeScript = GetDeserializationScript(GetType().GetConstructor(ConstructorArguments.Select(x => x.GetType()).ToArray()));

            var psExecutor = new PowerShellExecutor(server);
            var script = string.Format(@"
Param({3})
add-type -path {0}
add-type -path {5}
{4}
$operation = new-object -typename {1} -ArgumentList {6}
$logger = new-object -typename {2} -ArgumentList (Get-Host).UI
$operation.Execute($logger)
", remoteAssemblyFileName, typeName, loggerTypeName, scriptParams, deserializeScript, remoteJsonAssembly, argumentList);

            psExecutor.Execute(script, parameters);
        }

        private string GetDeserializationScript(ConstructorInfo constructor)
        {
            return constructor.GetParameters().Where(x => !CanBeSerializedByPowerShell(x.ParameterType)).Aggregate("", (current, param) => current + string.Format("${0} = [{1}]::DeserializeObject(${2}, [{3}])\n", param.Name, typeof(JsonConvert).FullName, param.Name + "__json", param.ParameterType.FullName));
        }

        private bool CanBeSerializedByPowerShell(Type type)
        {
            if (type.IsValueType) return true;
            if (type == typeof(string)) return true;
            if (type == typeof(byte[])) return true;
            if (type == typeof(DateTime)) return true;
            if (type == typeof(TimeSpan)) return true;
            if (type == typeof(Guid)) return true;
            if (type == typeof(Uri)) return true;
            if (type == typeof(Version)) return true;
            if (type == typeof(XmlDocument)) return true;
            if (type == typeof(SecureString)) return true;
            return false;
        }

        private IEnumerable<CommandParameter> GetPowerShellParameters(List<object> constructorArguments, Type type)
        {
            var constructor = type.GetConstructor(constructorArguments.Select(x => x.GetType()).ToArray());
            if (constructor != null)
            {
                return GetPowerShellParameters(constructorArguments, constructor);
            }
            return new List<CommandParameter>();
        }

        private IEnumerable<CommandParameter> GetPowerShellParameters(List<object> constructorArguments, ConstructorInfo constructor)
        {
            return constructor
                .GetParameters()
                .Select(parameter => CanBeSerializedByPowerShell(parameter.ParameterType) ?
                    new CommandParameter(parameter.Name, constructorArguments[parameter.Position]) :
                    new CommandParameter(parameter.Name + "__json", JsonConvert.SerializeObject(constructorArguments[parameter.Position]))
                );
        }

        public abstract string Name { get; }

        public void DryRun()
        {
        }

        public bool IsValid(Notification notification)
        {
            try
            {
                var constructor = GetType().GetConstructor(ConstructorArguments.Select(x => x.GetType()).ToArray());
                if (constructor == null)
                {
                    notification.AddError(new ValidationError(string.Format("Failed to extract constructor for operation {0}. Check the order of the parameters going into the base class. The order needs to be the same is in your constructor", GetType().Name)));
                    return false;
                }
            }
            catch(Exception ex)
            {
                notification.AddError(new ValidationError(string.Format("Failed to extract constructor for operation {0}. Error message: {1}", GetType().Name, ex.Message)));
                return false;
            }
            return true;
        }
    }
}
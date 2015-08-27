using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation.Runspaces;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;

namespace ConDep.Dsl.Remote
{
    internal class CredSSPHandler : IDisposable
    {
        private const string REG_KEY_ALLOW_FRESH_CREDENTIALS = @"SOFTWARE\Policies\Microsoft\Windows\CredentialsDelegation\AllowFreshCredentials";
        private const string REG_KEY_ALLOW_FRESH_CREDENTIALS_WHEN_NTLM_ONLY = @"SOFTWARE\Policies\Microsoft\Windows\CredentialsDelegation\AllowFreshCredentialsWhenNTLMOnly";
        private readonly List<Action> _cleanupFunctions = new List<Action>();

        private readonly ServerConfig _server;
        private readonly WSManConnectionInfo _connectionInfo;

        public CredSSPHandler(WSManConnectionInfo connectionInfo, ServerConfig server)
        {
            _server = server;
            connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Credssp;

            EnableCredSSP();
        }

        private void EnableCredSSP()
        {
            EnableClientCredSSP();
            EnableServerCredSSP();
        }

        private void EnableClientCredSSP()
        {
            var localPsExecutor = new PowerShellExecutor();
            var result = localPsExecutor.ExecuteLocal(_server, @"get-item -Path wsman:\localhost\Client\Auth\CredSSP", mod => mod.LoadConDepModule = false).ToList();

            if (result.Count != 1) throw new ConDepCredSSPException();

            bool credSspEnabled;
            if (!Boolean.TryParse(result.First().Value, out credSspEnabled))
            {
                throw new ConDepCredSSPException("Unable to retreive CredSSP value for this client.");
            }

            if (!credSspEnabled)
            {
                Logger.Verbose("CredSSP for client not enabled. Temporarly enabling now for this execution.");
                localPsExecutor.ExecuteLocal(_server, @"set-item -path wsman:\localhost\Client\Auth\CredSSP -value 'true'", mod => mod.LoadConDepModule = false);

                _cleanupFunctions.Add(() => localPsExecutor.ExecuteLocal(_server, @"set-item -path wsman:\localhost\Client\Auth\CredSSP -value 'false'", mod => mod.LoadConDepModule = false));
            }

            EnableFreshCredentials(REG_KEY_ALLOW_FRESH_CREDENTIALS);
            if(!IsDomainUser())
            {
                EnableFreshCredentials(REG_KEY_ALLOW_FRESH_CREDENTIALS_WHEN_NTLM_ONLY);
            }
        }

        private void EnableFreshCredentials(string key)
        {
            Logger.Verbose(string.Format("Checking if registry key for fresh credentials {0} is set.", key));
            var regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, true);
            if (regKey == null)
            {
                Logger.Verbose("Key not set. Setting now.");
                regKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(key);
                Logger.Verbose("Setting value 1 to wsman/*");
                regKey.SetValue("1", "WSMAN/*");
                _cleanupFunctions.Add(() => Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(key));
            }
            else if (regKey.ValueCount == 0)
            {
                Logger.Verbose("Key found, but no values exists. Setting value 1 to wsman/*");
                regKey.SetValue("1", @"WSMAN/*");
                _cleanupFunctions.Add(() => regKey.DeleteValue("1"));
                return;
            }


            if (regKey.GetValueNames()
                .Select(valName => regKey.GetValue(valName) as string)
                .Where(val => !string.IsNullOrWhiteSpace(val))
                .Any(val => val.Equals(@"wsman/*", StringComparison.InvariantCultureIgnoreCase)))
            {
                Logger.Verbose("Value for wsman/* found.");
                return;
            }

            var credSspRuleNumber = (regKey.ValueCount + 1).ToString(CultureInfo.InvariantCulture);
            Logger.Verbose("Value for wsman/* not found. Adding value with index " + credSspRuleNumber + " now.");
            regKey.SetValue(credSspRuleNumber, @"WSMAN/*");

            _cleanupFunctions.Add(() => regKey.DeleteValue(credSspRuleNumber));
        }

        private bool IsDomainUser()
        {
            Logger.Verbose(string.Format("Checking if {0} is a domain user.", _server.DeploymentUser.UserName));

            var split = _server.DeploymentUser.UserName.Split('\\');
            if (split.Length == 2)
            {
                if(split[0] != ".") return true;
            }
            return false;
        }

        private void EnableServerCredSSP()
        {
            var executor = new PowerShellExecutor();
            var result = executor.Execute(_server, @"get-item -path wsman:\localhost\Service\Auth\CredSSP", mod => mod.LoadConDepModule = false, logOutput: false).ToList();

            if (result.Count != 1) throw new ConDepCredSSPException();

            bool credSspEnabled;
            if (!Boolean.TryParse(result.First().Value, out credSspEnabled))
            {
                throw new ConDepCredSSPException("Unable to retreive CredSSP value from server.");
            }

            if (!credSspEnabled)
            {
                Logger.Verbose("CredSSP for server not enabled. Temporarly enabling now for this execution.");
                executor.Execute(_server, @"set-item -path wsman:\localhost\Service\Auth\CredSSP -value ""true"" -force", mod => mod.LoadConDepModule = false, logOutput: false);
                _cleanupFunctions.Add(() => executor.Execute(_server, @"set-item -path wsman:\localhost\Service\Auth\CredSSP -value ""false"" -force", mod => mod.LoadConDepModule = false, logOutput: false));
            }
        }

        public void Dispose()
        {
            foreach (var cleanupMethod in _cleanupFunctions)
            {
                cleanupMethod();
            }
        }
    }
}
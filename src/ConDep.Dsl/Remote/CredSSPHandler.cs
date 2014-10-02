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
        private List<Action> _cleanupFunctions = new List<Action>();
 
        private readonly ServerConfig _server;
        private readonly WSManConnectionInfo _connectionInfo;
        private bool _originalClientCredSSPValue;
        private bool _originalServerCredSSPValue;

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
            var localExecutor = new PowerShellExecutor(_server) {LoadConDepModule = false};

            var result = localExecutor.ExecuteLocal(@"get-item -Path wsman:\localhost\Client\Auth\CredSSP").ToList();

            if (result.Count != 1) throw new ConDepCredSSPException();

            bool value;
            if (Boolean.TryParse(result.First().Value, out value))
            {
                _originalClientCredSSPValue = value;
            }

            if (!_originalClientCredSSPValue)
            {
                Logger.Verbose("CredSSP for client not enabled. Temporarly enabling now for this execution.");
                localExecutor.ExecuteLocal(@"set-item -path wsman:\localhost\Client\Auth\CredSSP -value 'true'");

                _cleanupFunctions.Add(() => localExecutor.ExecuteLocal(@"set-item -path wsman:\localhost\Client\Auth\CredSSP -value 'false'"));
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
            var domain = Impersonator.GetDomain(_server.DeploymentUser.UserName);
            Logger.Verbose(string.Format("Domain found was: {0}", domain));
            var isDomainUser = !string.IsNullOrWhiteSpace(domain) && domain != ".";
            Logger.Verbose(string.Format("Is user {0} domain user: {1}", _server.DeploymentUser.UserName, isDomainUser));
            return isDomainUser;
        }

        private void EnableServerCredSSP()
        {
            var executor = new PowerShellExecutor(_server) { LoadConDepModule = false };
            var result = executor.Execute(@"get-item -path wsman:\localhost\Service\Auth\CredSSP", logOutput: false).ToList();

            if (result.Count != 1) throw new ConDepCredSSPException();

            bool value;
            if (Boolean.TryParse(result.First().Value, out value))
            {
                _originalServerCredSSPValue = value;
            }

            if (!_originalServerCredSSPValue)
            {
                Logger.Verbose("CredSSP for server not enabled. Temporarly enabling now for this execution.");
                executor.Execute(@"set-item -path wsman:\localhost\Service\Auth\CredSSP -value ""true"" -force", logOutput: false);
                _cleanupFunctions.Add(() => executor.Execute(@"set-item -path wsman:\localhost\Service\Auth\CredSSP -value ""false"" -force", logOutput: false));
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
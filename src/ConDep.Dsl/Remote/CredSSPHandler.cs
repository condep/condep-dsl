using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using Microsoft.Win32;

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

        //private void DisableCredSSP()
        //{
        //    DisableClientCredSSP();
        //    DisableServerCredSSP();
        //}

        //private void DisableServerCredSSP()
        //{
        //    if (!_originalServerCredSSPValue)
        //    {
        //        var executor = new PowerShellExecutor(_server) { LoadConDepModule = false };
        //        Logger.Verbose("Disabling CredSSP for server since it was disabled to begin with.");
        //        executor.Execute(@"set-item -path wsman:\localhost\Service\Auth\CredSSP -value ""false""", logOutput: false);
        //    }
        //}

        //private void DisableClientCredSSP()
        //{
        //    if (!_originalClientCredSSPValue)
        //    {
        //        Logger.Verbose("Disabling CredSSP for client since it was disabled to begin with.");
        //        using (var ps = PowerShell.Create())
        //        {
        //            ps.AddScript(@"Disable-WSManCredSSP -Role Client");
        //            ps.Invoke();
        //        }
        //    }
        //}


        private void EnableClientCredSSP()
        {
            //if Domain
            //  if HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CredentialsDelegation\AllowFreshCredentials Exists
            //      if
            //  Add credssp settings to HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CredentialsDelegation\AllowFreshCredentials
            //else
            //  Add credssp setti to HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CredentialsDelegation\AllowFreshCredentialsWhenNTLMOnly
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

            if (IsDomainComputer())
            {
                EnableFreshCredentials(REG_KEY_ALLOW_FRESH_CREDENTIALS);
            }
            else
            {
                EnableFreshCredentials(REG_KEY_ALLOW_FRESH_CREDENTIALS_WHEN_NTLM_ONLY);
            }
        }

        private void EnableFreshCredentials(string key)
        {

            var regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key, true);
            if (regKey == null)
            {
                regKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(key);
                regKey.SetValue("1", @"WSMAN/*");
                _cleanupFunctions.Add(() => Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(key));
            }
            else if (regKey.ValueCount == 0)
            {
                regKey.SetValue("1", @"WSMAN/*");
                _cleanupFunctions.Add(() => regKey.DeleteValue("1"));
                return;
            }


            if (regKey.GetValueNames()
                .Select(valName => regKey.GetValue(valName) as string)
                .Where(val => !string.IsNullOrWhiteSpace(val))
                .Any(val => val.Equals(@"wsman/*", StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            var credSspRuleNumber = (regKey.ValueCount + 1).ToString(CultureInfo.InvariantCulture);
            regKey.SetValue(credSspRuleNumber, @"WSMAN/*");

            _cleanupFunctions.Add(() => regKey.DeleteValue(credSspRuleNumber));
        }

        private bool IsDomainComputer()
        {
            return !Environment.UserDomainName.Equals(Environment.MachineName, StringComparison.InvariantCultureIgnoreCase);
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

    public class ConDepCredSSPException : Exception
    {
    }
}
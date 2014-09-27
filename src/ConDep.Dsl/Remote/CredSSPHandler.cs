using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;

namespace ConDep.Dsl.Remote
{
    public class CredSSPHandler : IDisposable
    {
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

        private void DisableCredSSP()
        {
            DisableClientCredSSP();
            DisableServerCredSSP();
        }

        private void DisableServerCredSSP()
        {
            if (!_originalServerCredSSPValue)
            {
                var executor = new PowerShellExecutor(_server) { LoadConDepModule = false };
                Logger.Verbose("Disabling CredSSP for server since it was disabled to begin with.");
                executor.Execute(@"set-item -path wsman:\localhost\Service\Auth\CredSSP -value ""false""", logOutput: false);
            }
        }

        private void DisableClientCredSSP()
        {
            if (!_originalClientCredSSPValue)
            {
                Logger.Verbose("Disabling CredSSP for client since it was disabled to begin with.");
                using (var ps = PowerShell.Create())
                {
                    ps.AddScript(@"Disable-WSManCredSSP -Role Client");
                    ps.Invoke();
                }
            }
        }


        private void EnableClientCredSSP()
        {
            using (var ps = PowerShell.Create())
            {
                ps.AddScript(@"get-item -Path wsman:\localhost\Client\Auth\CredSSP");
                var result = ps.Invoke();
                if (result.Count == 1)
                {
                    bool value;
                    if (Boolean.TryParse(result[0].ToString(), out value))
                    {
                        _originalClientCredSSPValue = value;
                    }
                }
            }

            if (!_originalClientCredSSPValue)
            {
                Logger.Verbose("CredSSP for client not enabled. Temporarly enabling now for this execution.");
                using (var ps = PowerShell.Create())
                {
                    ps.AddScript(@"Enable-WSManCredSSP -Role Client");
                    ps.Invoke();
                }
            }
        }

        private void EnableServerCredSSP()
        {
            var executor = new PowerShellExecutor(_server) { LoadConDepModule = false };
            var result = executor.Execute(@"get-item -path wsman:\localhost\Service\Auth\CredSSP", logOutput: false);

            if (result.Count() == 1)
            {
                bool value;
                if (Boolean.TryParse(result.First().ToString(), out value))
                {
                    _originalServerCredSSPValue = value;
                }
            }

            if (!_originalServerCredSSPValue)
            {
                Logger.Verbose("CredSSP for server not enabled. Temporarly enabling now for this execution.");
                executor.Execute(@"set-item -path wsman:\localhost\Service\Auth\CredSSP -value ""true""", logOutput: false);
            }

        }

        public void Dispose()
        {
            DisableCredSSP();
        }
    }
}
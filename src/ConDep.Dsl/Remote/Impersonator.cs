using System;
using System.ComponentModel;
using System.Security.Principal;

namespace ConDep.Dsl.Remote
{
    public class Impersonator : IDisposable
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string _domain;
        private bool _disposed;
        private IntPtr _tokenDuplicate;
        private WindowsImpersonationContext _impersonationContext;
        private IntPtr _token;

        public Impersonator(string username, string password)
        {
            if (username == null || password == null)
                return;

            _username = GetUserName(username);
            _domain = GetDomain(username);
            _password = password;

            try
            {
                Initialize();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public static string GetUserName(string username)
        {
            var split = username.Split('\\');
            if (split.Length == 1)
            {
                return username;
            }
            if (split.Length == 2)
            {
                return split[1];
            }
            throw new NotSupportedException("Username format not supported. More than one '\\' found in username string.");
        }

        public static string GetDomain(string username)
        {
            var split = username.Split('\\');
            if (split.Length == 1)
            {
                return "";
            }
            if (split.Length == 2)
            {
                return split[0];
            }
            throw new NotSupportedException("Username format not supported. More than one '\\' found in username string.");
        }

        private void Initialize()
        {
            if (!ImpersinatorNativeMethods.LogonUser(
                _username,
                _domain,
                _password,
                ImpersinatorNativeMethods.LogonType.NewCredentials,
                ImpersinatorNativeMethods.LogonProvider.Default,
                out _token))
            {
                throw new Win32Exception();
            }

            if (!ImpersinatorNativeMethods.DuplicateToken(
                _token,
                ImpersinatorNativeMethods.SecurityImpersonationLevel.Impersonation,
                out _tokenDuplicate))
            {
                throw new Win32Exception();
            }

            _impersonationContext = new WindowsIdentity(_tokenDuplicate).Impersonate();
            // Do stuff with your share here.
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    if(_impersonationContext != null)
                    {
                        _impersonationContext.Undo();
                        _impersonationContext.Dispose();
                    }

                    if (_tokenDuplicate != IntPtr.Zero)
                    {
                        if (!ImpersinatorNativeMethods.CloseHandle(_tokenDuplicate))
                        {
                            // Uncomment if you need to know this case.
                            ////throw new Win32Exception();
                        }
                    }

                    if (_token != IntPtr.Zero)
                    {
                        if (!ImpersinatorNativeMethods.CloseHandle(_token))
                        {
                            // Uncomment if you need to know this case.
                            ////throw new Win32Exception();
                        }
                    }
                }

                _disposed = true;
    
            }
        }

        ~Impersonator()
        {
            Dispose(false);
        }
    }
}

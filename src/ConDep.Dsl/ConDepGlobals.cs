using System;
using System.Collections.Generic;
using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    public static class ConDepGlobals
    {
        public static Guid ExecId = Guid.NewGuid();
        public static readonly Dictionary<string, IServerConfig> ServersWithPreOps = new Dictionary<string, IServerConfig>();

        public static void Reset()
        {
            ExecId = Guid.NewGuid();
            ServersWithPreOps.Clear();
        }
    }
}
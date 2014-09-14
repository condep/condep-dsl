using System;
using System.Collections.Generic;

namespace ConDep.Dsl.Config
{
    [Serializable]
    public class OperationConfig
    {
        public string OperationName { get; set; }
        public Dictionary<string, string> Config { get; set; }
    }
}
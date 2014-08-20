using System;
using System.Diagnostics;
using System.Management.Automation.Host;

namespace ConDep.Dsl.Logging
{
    public class RemotePowerShellLogger : LoggerBase
    {
        private readonly PSHostUserInterface _psHostUserInterface;

        public RemotePowerShellLogger(PSHostUserInterface psHostUserInterface)
        {
            _psHostUserInterface = psHostUserInterface;
        }

        public override void Progress(string message, params object[] formatArgs)
        {
            
        }

        public override void ProgressEnd()
        {
        }

        public override void LogSectionStart(string name)
        {
        }

        public override void LogSectionEnd(string name)
        {
        }

        public override TraceLevel TraceLevel { get; set; }

        public override void Log(string message, Exception ex, TraceLevel traceLevel, params object[] formatArgs)
        {
            switch (traceLevel)
            {
                case TraceLevel.Error:
                    _psHostUserInterface.WriteErrorLine(string.Format(message, formatArgs));
                    break;
                case TraceLevel.Info:
                    _psHostUserInterface.WriteLine(string.Format(message, formatArgs));
                    break;
                case TraceLevel.Verbose:
                    _psHostUserInterface.WriteVerboseLine(string.Format(message, formatArgs));
                    break;
                case TraceLevel.Warning:
                    _psHostUserInterface.WriteWarningLine(string.Format(message, formatArgs));
                    break;
                default:
                    _psHostUserInterface.WriteLine(string.Format(message, formatArgs));
                    break;
            }
        }
    }
}
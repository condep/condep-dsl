using System;
using System.Diagnostics;

namespace ConDep.Dsl.Logging
{
    public class Logger
    {
        private bool? _tcServiceExist;

        public static void Initialize(ILogForConDep log)
        {
            InternalLogger = log;
        }
           
        public static ILogForConDep InternalLogger { get; private set; }

        public void ResolveLogger(IResolveLogger resolver)
        {
            Initialize(resolver.GetLogger());
        }
      
        public static void Info(string message, params object[] formatArgs)
        {
            InternalLogger.Info(message, formatArgs);
        }

        public static void Info(string message, Exception ex, params object[] formatArgs)
        {
            InternalLogger.Info(message, ex, formatArgs);
        }

        public static void Warn(string message, params object[] formatArgs)
        {
            InternalLogger.Warn(message, formatArgs);
        }

        public static void Warn(string message, Exception ex, params object[] formatArgs)
        {
            InternalLogger.Warn(message, ex, formatArgs);
        }

        public static void Error(string message, params object[] formatArgs)
        {
            InternalLogger.Error(message, null, formatArgs);
        }

        public static void Error(string message, Exception ex, params object[] formatArgs)
        {
            InternalLogger.Error(message, ex, formatArgs);
        }

        public static void Verbose(string message, params object[] formatArgs)
        {
            InternalLogger.Verbose(message, formatArgs);
        }

        public static void Verbose(string message, Exception ex, params object[] formatArgs)
        {
            InternalLogger.Verbose(message, ex, formatArgs);
        }

        public static void Progress(string message, params object[] formatArgs)
        {
            InternalLogger.Progress(message, formatArgs);
        }

        public static void ProgressEnd()
        {
            InternalLogger.ProgressEnd();
        }

        public static void Log(string message, TraceLevel traceLevel, params object[] formatArgs)
        {
            InternalLogger.Log(message, traceLevel, formatArgs);
        }

        public static void Log(string message, Exception ex, TraceLevel traceLevel, params object[] formatArgs)
        {
            InternalLogger.Log(message, ex, traceLevel, formatArgs);
        }

        public static TraceLevel TraceLevel
        {
            get { return InternalLogger.TraceLevel; }
            set { InternalLogger.TraceLevel = value; }
        }

        public static ConsoleColor BackgroundColor { get; set; }
        public static ConsoleColor ForegroundColor { get; set; }

        public static void LogSectionStart(string name)
        {
            InternalLogger.LogSectionStart(name);
        }

        public static void LogSectionEnd(string name)
        {
            InternalLogger.LogSectionEnd(name);
        }

        public static ILogForConDep LogInstance
        {
            get { return InternalLogger; }
        }

        public static void WithLogSection(string sectionName, Action action)
        {
            try
            {
                LogSectionStart(sectionName);
                action();
            }
            finally
            {
                LogSectionEnd(sectionName);
            }
        }

        public static T WithLogSection<T>(string sectionName, Func<T> action)
        {
            try
            {
                LogSectionStart(sectionName);
                return action();
            }
            finally
            {
                LogSectionEnd(sectionName);
            }
        }

    }

    public interface IResolveLogger
    {
        ILogForConDep GetLogger();
    }
}
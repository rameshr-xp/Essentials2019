

using System;
using System.Linq;
using System.Reflection;

namespace AzureIotHub.Services.Diagnostics
{
    public enum LogLevel
    {
        Debug = 10,
        Info = 20,
        Warn = 30,
        Error = 40
    }

    public interface ILogger
    {
        // The following 4 methods allow to log a message, capturing the context
        // (i.e. the method where the log message is generated)

        void Debug(string message, Action context);
        void Info(string message, Action context);
        void Warn(string message, Action context);
        void Error(string message, Action context);

        // The following 4 methods allow to log a message and some data,
        // capturing the context (i.e. the method where the log message is generated)

        void Debug(string message, Func<object> context);
        void Info(string message, Func<object> context);
        void Warn(string message, Func<object> context);
        void Error(string message, Func<object> context);
    }

    public class Logger : ILogger
    {
        private readonly string processId;
        private readonly LogLevel loggingLevel;

        public Logger(string processId, LogLevel loggingLevel)
        {
            this.processId = processId;
            this.loggingLevel = loggingLevel;
        }

        // The following 4 methods allow to log a message, capturing the context
        // (i.e. the method where the log message is generated)
        public void Debug(string message, Action context)
        {
            if (loggingLevel > LogLevel.Debug)
            {
                return;
            }

            Write("DEBUG", context.GetMethodInfo(), message);
        }

        public void Info(string message, Action context)
        {
            if (loggingLevel > LogLevel.Info)
            {
                return;
            }

            Write("INFO", context.GetMethodInfo(), message);
        }

        public void Warn(string message, Action context)
        {
            if (loggingLevel > LogLevel.Warn)
            {
                return;
            }

            Write("WARN", context.GetMethodInfo(), message);
        }

        public void Error(string message, Action context)
        {
            if (loggingLevel > LogLevel.Error)
            {
                return;
            }

            Write("ERROR", context.GetMethodInfo(), message);
        }

        // The following 4 methods allow to log a message and some data,
        // capturing the context (i.e. the method where the log message is generated)
        public void Debug(string message, Func<object> context)
        {
            if (loggingLevel > LogLevel.Debug)
            {
                return;
            }

            if (!string.IsNullOrEmpty(message))
            {
                message += ", ";
            }

            message += Serialization.Serialize(context.Invoke());

            Write("DEBUG", context.GetMethodInfo(), message);
        }

        public void Info(string message, Func<object> context)
        {
            if (loggingLevel > LogLevel.Info)
            {
                return;
            }

            if (!string.IsNullOrEmpty(message))
            {
                message += ", ";
            }

            message += Serialization.Serialize(context.Invoke());

            Write("INFO", context.GetMethodInfo(), message);
        }

        public void Warn(string message, Func<object> context)
        {
            if (loggingLevel > LogLevel.Warn)
            {
                return;
            }

            if (!string.IsNullOrEmpty(message))
            {
                message += ", ";
            }

            message += Serialization.Serialize(context.Invoke());

            Write("WARN", context.GetMethodInfo(), message);
        }

        public void Error(string message, Func<object> context)
        {
            if (loggingLevel > LogLevel.Error)
            {
                return;
            }

            if (!string.IsNullOrEmpty(message))
            {
                message += ", ";
            }

            message += Serialization.Serialize(context.Invoke());

            Write("ERROR", context.GetMethodInfo(), message);
        }

        /// <summary>
        /// Log the message and information about the context, cleaning up
        /// and shortening the class name and method name (e.g. removing
        /// symbols specific to .NET internal implementation)
        /// </summary>
        private void Write(string level, MethodInfo context, string text)
        {
            // Extract the Class Name from the context
            string classname = "";
            if (context.DeclaringType != null)
            {
                classname = context.DeclaringType.FullName;
            }
            classname = classname.Split(new[] { '+' }, 2).First();
            classname = classname.Split('.').LastOrDefault();

            // Extract the Method Name from the context
            string methodname = context.Name;
            methodname = methodname.Split(new[] { '>' }, 2).First();
            methodname = methodname.Split(new[] { '<' }, 2).Last();

            string time = DateTimeOffset.UtcNow.ToString("u");
            Console.WriteLine($"[{processId}][{time}][{level}][{classname}:{methodname}] {text}");
        }
    }
}

using System;
using System.Fabric;
using PipServices.Commons.Log;
using PipServices.Commons.Refer;

namespace PipServices.Azure.Logs
{
    public sealed class ServiceEventLogger : AbstractLogger
    {
        static ServiceEventLogger()
        {
            Descriptor = new Descriptor("pip-commons", "logger", "service-fabric", "1.0");
        }

        private void WriteWithoutContext(string correlationId, LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Fatal:
                    LogEventSource.Current.Fatal(correlationId, message);
                    break;
                case LogLevel.Error:
                    LogEventSource.Current.Error(correlationId, message);
                    break;
                case LogLevel.Warn:
                    LogEventSource.Current.Warn(correlationId, message);
                    break;
                case LogLevel.Info:
                    LogEventSource.Current.Info(correlationId, message);
                    break;
                case LogLevel.Debug:
                    LogEventSource.Current.Debug(correlationId, message);
                    break;
                case LogLevel.Trace:
                    LogEventSource.Current.Trace(correlationId, message);
                    break;
            }
        }

        protected override void PerformWrite(string correlationId, LogLevel level, Exception error, string message)
        {
            if (Level < level) return;

            var output = LogFormatter.Format(level, message);
            if (correlationId != null)
                output += ", correlated to " + correlationId;

            var context = References?.GetOneOptional(new Descriptor("*", "context", "azure", "*"));

            if (context is StatefulServiceContext)
                ServiceEventSource.Current.Event(level, (StatefulServiceContext)context, output);
            else if (context is StatelessServiceContext)
                ServiceEventSource.Current.Event(level, (StatelessServiceContext)context, output);
            else
                WriteWithoutContext(correlationId, level, message);
        }
    }
}

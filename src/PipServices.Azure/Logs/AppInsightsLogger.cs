using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using PipServices.Commons.Config;
using PipServices.Commons.Log;
using PipServices.Commons.Refer;

namespace PipServices.Azure.Logs
{
    public sealed class AppInsightsLogger : AbstractLogger
    {
        private readonly TelemetryClient _client;

        static AppInsightsLogger()
        {
            Descriptor = new Descriptor("pip-commons", "logger", "app-insights", "1.0");
        }

        public AppInsightsLogger()
        {
            _client = new TelemetryClient();
        }

        private SeverityLevel LevelToSeverity(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Fatal:
                    return SeverityLevel.Critical;
                case LogLevel.Error:
                    return SeverityLevel.Error;
                case LogLevel.Warn:
                    return SeverityLevel.Warning;
                case LogLevel.Info:
                    return SeverityLevel.Information;
                case LogLevel.Debug:
                    return SeverityLevel.Verbose;
                case LogLevel.Trace:
                    return SeverityLevel.Verbose;
            }

            return SeverityLevel.Verbose;
        }

        protected override void PerformWrite(string correlationId, LogLevel level, Exception error, string message)
        {
            if (_client == null) return;

            message = LogFormatter.Format(level, message);

            if (error != null)
            {
                message = message != null ? message + ": " : "Error: ";
                message = message + error.Message + " StackTrace: " + error.StackTrace;
            }

            if (correlationId != null)
            {
                var props = new Dictionary<string, string> { { "CorrelationId", correlationId } };

                _client.TrackTrace(message, LevelToSeverity(level), props);

                if (error != null)
                    _client.TrackException(error, props);
            }
            else
            {
                _client.TrackTrace(message, LevelToSeverity(level));

                if (error != null)
                    _client.TrackException(error);
            }
        }

        public void Dump()
        {
            _client.Flush();
        }

        public override void Configure(ConfigParams config)
        {
            base.Configure(config);

            if (config.ContainsKey("instrumentation_key"))
                TelemetryConfiguration.Active.InstrumentationKey = config.GetAsString("instrumentation_key");
        }
    }
}

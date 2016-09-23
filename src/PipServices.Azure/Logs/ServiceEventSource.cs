using System;
using System.Diagnostics.Tracing;
using System.Fabric;
using System.Threading.Tasks;
using PipServices.Commons.Log;

namespace PipServices.Azure.Logs
{
    [EventSource(Name = "pip-azure")]
    public sealed class ServiceEventSource : EventSource
    {
        public static readonly ServiceEventSource Current = new ServiceEventSource();

        static ServiceEventSource()
        {
            // A workaround for the problem where ETW activities do not get tracked until Tasks infrastructure is initialized.
            // This problem will be fixed in .NET Framework 4.6.2.
            Task.Run(() => { });
        }

        // Instance constructor is private to enforce singleton semantics
        private ServiceEventSource()
        {
        }

        [NonEvent]
        public void Message(string message, params object[] args)
        {
            if (IsEnabled())
            {
                var finalMessage = string.Format(message, args);
                Message(finalMessage);
            }
        }

        private const int MessageEventId = 1;
        [Event(MessageEventId, Level = EventLevel.Informational, Message = "{0}")]
        public void Message(string message)
        {
            if (IsEnabled())
            {
                WriteEvent(MessageEventId, message);
            }
        }

        private
#if UNSAFE
            unsafe
#endif
            void Event(
            int eventId,
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName,
            Guid partitionId,
            long replicaOrInstanceId,
            string nodeName,
            string message)
        {
#if !UNSAFE
            WriteEvent(
                    eventId,
                    applicationTypeName,
                    applicationName,
                    serviceTypeName,
                    serviceName,
                    partitionId,
                    replicaOrInstanceId,
                    nodeName,
                    message);
#else
                const int numArgs = 10;
                fixed (char* pActorType = actorType, pActorId = actorId, pApplicationTypeName = applicationTypeName, pApplicationName = applicationName, pServiceTypeName = serviceTypeName, pServiceName = serviceName, pNodeName = nodeName, pMessage = message)
                {
                    EventData* eventData = stackalloc EventData[numArgs];
                    eventData[0] = new EventData { DataPointer = (IntPtr) pApplicationTypeName, Size = SizeInBytes(applicationTypeName) };
                    eventData[1] = new EventData { DataPointer = (IntPtr) pApplicationName, Size = SizeInBytes(applicationName) };
                    eventData[2] = new EventData { DataPointer = (IntPtr) pServiceTypeName, Size = SizeInBytes(serviceTypeName) };
                    eventData[3] = new EventData { DataPointer = (IntPtr) pServiceName, Size = SizeInBytes(serviceName) };
                    eventData[4] = new EventData { DataPointer = (IntPtr) (&partitionId), Size = sizeof(Guid) };
                    eventData[5] = new EventData { DataPointer = (IntPtr) (&replicaOrInstanceId), Size = sizeof(long) };
                    eventData[6] = new EventData { DataPointer = (IntPtr) pNodeName, Size = SizeInBytes(nodeName) };
                    eventData[7] = new EventData { DataPointer = (IntPtr) pMessage, Size = SizeInBytes(message) };

                    WriteEventCore(eventId, numArgs, eventData);
                }
#endif
        }

        private const int CriticalEventId = 101;
        [Event(CriticalEventId, Level = EventLevel.Critical, Message = "{7}")]
        private void CriticalEvent(
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName,
            Guid partitionId,
            long replicaOrInstanceId,
            string nodeName,
            string message)
        {
            Event(CriticalEventId, applicationTypeName, applicationName, serviceTypeName, serviceName, partitionId,
                replicaOrInstanceId, nodeName, message);
        }

        private const int ErrorEventId = 102;
        [Event(ErrorEventId, Level = EventLevel.Error, Message = "{7}")]
        private void ErrorEvent(
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName,
            Guid partitionId,
            long replicaOrInstanceId,
            string nodeName,
            string message)
        {
            Event(ErrorEventId, applicationTypeName, applicationName, serviceTypeName, serviceName, partitionId,
                replicaOrInstanceId, nodeName, message);
        }

        private const int WarningEventId = 103;
        [Event(WarningEventId, Level = EventLevel.Warning, Message = "{7}")]
        private void WarningEvent(
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName,
            Guid partitionId,
            long replicaOrInstanceId,
            string nodeName,
            string message)
        {
            Event(WarningEventId, applicationTypeName, applicationName, serviceTypeName, serviceName, partitionId,
                replicaOrInstanceId, nodeName, message);
        }

        private const int InformationalEventId = 104;
        [Event(InformationalEventId, Level = EventLevel.Informational, Message = "{7}")]
        private void InformationalEvent(
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName,
            Guid partitionId,
            long replicaOrInstanceId,
            string nodeName,
            string message)
        {
            Event(InformationalEventId, applicationTypeName, applicationName, serviceTypeName, serviceName, partitionId,
                replicaOrInstanceId, nodeName, message);
        }

        private const int DebugEventId = 105;
        [Event(DebugEventId, Level = EventLevel.Verbose, Message = "{7}")]
        private void DebugEvent(
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName,
            Guid partitionId,
            long replicaOrInstanceId,
            string nodeName,
            string message)
        {
            Event(DebugEventId, applicationTypeName, applicationName, serviceTypeName, serviceName, partitionId,
                replicaOrInstanceId, nodeName, message);
        }

        private const int TraceEventId = 106;
        [Event(TraceEventId, Level = EventLevel.Verbose, Message = "{7}")]
        private void TraceEvent(
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName,
            Guid partitionId,
            long replicaOrInstanceId,
            string nodeName,
            string message)
        {
            Event(TraceEventId, applicationTypeName, applicationName, serviceTypeName, serviceName, partitionId,
                replicaOrInstanceId, nodeName, message);
        }

        [NonEvent]
        public void Event(LogLevel level, StatefulServiceContext context, string message, params object[] args)
        {
            if (!IsEnabled() || context == null || context.CodePackageActivationContext == null) return;

            var finalMessage = string.Format(message, args);

            Action<string, string, string, string, Guid, long, string, string> func;

            switch (level)
            {
                case LogLevel.Fatal:
                    func = CriticalEvent;
                    break;
                case LogLevel.Error:
                    func = ErrorEvent;
                    break;
                case LogLevel.Warn:
                    func = WarningEvent;
                    break;
                case LogLevel.Debug:
                    func = DebugEvent;
                    break;
                case LogLevel.Trace:
                    func = TraceEvent;
                    break;
                default:
                    func = InformationalEvent;
                    break;
            }

            func(
                context.CodePackageActivationContext.ApplicationTypeName,
                context.CodePackageActivationContext.ApplicationName,
                context.ServiceTypeName,
                context.ServiceName.ToString(),
                context.PartitionId,
                context.ReplicaId,
                context.NodeContext.NodeName,
                finalMessage);
        }

        [NonEvent]
        public void Event(LogLevel level, StatelessServiceContext context, string message, params object[] args)
        {
            if (!IsEnabled() || context == null || context.CodePackageActivationContext == null) return;

            var finalMessage = string.Format(message, args);

            Action<string, string, string, string, Guid, long, string, string> func;

            switch (level)
            {
                case LogLevel.Fatal:
                    func = CriticalEvent;
                    break;
                case LogLevel.Error:
                    func = ErrorEvent;
                    break;
                case LogLevel.Warn:
                    func = WarningEvent;
                    break;
                case LogLevel.Debug:
                    func = DebugEvent;
                    break;
                case LogLevel.Trace:
                    func = TraceEvent;
                    break;
                default:
                    func = InformationalEvent;
                    break;
            }

            func(
                context.CodePackageActivationContext.ApplicationTypeName,
                context.CodePackageActivationContext.ApplicationName,
                context.ServiceTypeName,
                context.ServiceName.ToString(),
                context.PartitionId,
                context.InstanceId,
                context.NodeContext.NodeName,
                finalMessage);
        }
    }
}

using PipServices.Commons.Build;
using PipServices.Commons.Refer;
using PipServices.Azure.Messaging;
using PipServices.Azure.Config;
using PipServices.Azure.Count;
using PipServices.Azure.Log;
using PipServices.Azure.Lock;
using PipServices.Azure.Metrics;

namespace PipServices.Azure.Build
{
    public class DefaultAzureFactory : Factory
    {
        public static Descriptor Descriptor = new Descriptor("pip-services", "factory", "azure", "default", "1.0");

        public static Descriptor StorageMessageQueueDescriptor = new Descriptor("pip-services", "message-queue", "storage-message-queue", "default", "1.0");
        // TODO: Not ready for .net core 2.0, see  https://github.com/Azure/azure-service-bus-dotnet/issues/65
        //public static Descriptor ServiceBusMessageQueueDescriptor = new Descriptor("pip-services", "message-queue", "servicebus-message-queue", "*", "1.0");
        //public static Descriptor ServiceBusMessageTopicDescriptor = new Descriptor("pip-services", "message-queue", "servicebus-message-topic", "*", "1.0");
        public static Descriptor KeyVaultConfigReaderDescriptor = new Descriptor("pip-services", "config-reader", "key-vault", "*", "1.0");
        public static Descriptor AppInsightsCountersDescriptor = new Descriptor("pip-services", "counters", "app-insights", "*", "1.0");
        public static Descriptor AppInsightsLoggerDescriptor = new Descriptor("pip-services", "logger", "app-insights", "*", "1.0");
        public static Descriptor CloudStorageTableLockDescriptor = new Descriptor("pip-services", "lock", "storage-table", "*", "1.0");
        public static Descriptor CosmosDbMetricsServiceDescriptor = new Descriptor("pip-services", "metrics-service", "cosmosdb", "*", "1.0");

        public DefaultAzureFactory()
        {
            RegisterAsType(StorageMessageQueueDescriptor, typeof(StorageMessageQueue));
            // TODO: Not ready for .net core 2.0, see  https://github.com/Azure/azure-service-bus-dotnet/issues/65
            //RegisterAsType(ServiceBusMessageQueueDescriptor, typeof(ServiceBusMessageQueue));
            //RegisterAsType(ServiceBusMessageTopicDescriptor, typeof(ServiceBusMessageTopic));
            RegisterAsType(KeyVaultConfigReaderDescriptor, typeof(KeyVaultConfigReader));
            RegisterAsType(AppInsightsCountersDescriptor, typeof(AppInsightsCounters));
            RegisterAsType(AppInsightsLoggerDescriptor, typeof(AppInsightsLogger));
            RegisterAsType(CloudStorageTableLockDescriptor, typeof(CloudStorageTableLock));
            RegisterAsType(CosmosDbMetricsServiceDescriptor, typeof(CosmosDbMetricsService));
        }
    }
}

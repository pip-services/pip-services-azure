using PipServices.Commons.Build;
using PipServices.Commons.Refer;
using PipServices.Azure.Messaging;
using PipServices.Azure.Config;
using PipServices.Azure.Count;
using PipServices.Azure.Log;

namespace PipServices.Azure.Build
{
    public class DefaultAzureFactory : Factory
    {
        public static Descriptor Descriptor = new Descriptor("pip-services", "factory", "azure", "default", "1.0");

        public static Descriptor StorageMessageQueueDescriptor = new Descriptor("pip-services", "message-queue", "storage-message-queue", "default", "1.0");
        public static Descriptor ServiceBusMessageQueueDescriptor = new Descriptor("pip-services", "message-queue", "servicebus-message-queue", "*", "1.0");
        public static Descriptor ServiceBusMessageTopicDescriptor = new Descriptor("pip-services", "message-queue", "servicebus-message-topic", "*", "1.0");
        public static Descriptor KeyVaultConfigReaderDescriptor = new Descriptor("pip-services", "config-reader", "key-vault", "*", "1.0");
        public static Descriptor AppInsightsCountersDescriptor = new Descriptor("pip-services", "counters", "app-insights", "*", "1.0");
        public static Descriptor AppInsightsLoggerDescriptor = new Descriptor("pip-services", "logger", "app-insights", "*", "1.0");

        public DefaultAzureFactory()
        {
            RegisterAsType(StorageMessageQueueDescriptor, typeof(StorageMessageQueue));
            RegisterAsType(ServiceBusMessageQueueDescriptor, typeof(ServiceBusMessageQueue));
            RegisterAsType(ServiceBusMessageTopicDescriptor, typeof(ServiceBusMessageTopic));
            RegisterAsType(KeyVaultConfigReaderDescriptor, typeof(KeyVaultConfigReader));
            RegisterAsType(AppInsightsCountersDescriptor, typeof(AppInsightsCounters));
            RegisterAsType(AppInsightsLoggerDescriptor, typeof(AppInsightsLogger));
        }
    }
}

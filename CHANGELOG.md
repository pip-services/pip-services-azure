# Azure components for Pip.Services in .NET Changelog

## <a name="2.2.0-2.2.11"></a> 2.2.0-2.2.11 (2019-08-01)
* **persistence** CosmosDB MongoDB partition persistence
* **persistence** CosmosDB Throughput Monitor
* **metrics** CosmosDB Metrics Service
* **lock** Cloud table storage lock

### Features
* Migrated to .NET Standard 2.0
* Updated references to:
  - Service Fabric 3.0 
  - WindowsAzure.ServiceBus 4.1 
  - WindowsAzure.Storage 9.1

## <a name="2.1.0"></a> 2.1.0 (2017-05-17)

### Features
* Migrated to Service Fabric 2.6 and latest versions of WindowsAzure.ServiceBus and WindowsAzure.Storage

## <a name="2.0.0"></a> 2.0.0 (2017-02-27)

### Breaking Changes
* Migrated to **pip-services** 2.0

## <a name="1.0.0"></a> 1.0.0 (2016-12-10)

Initial public release

### Features
* **auth** KeyVault client
* **config** Secure KeyVaultConfigReader
* **count** AppInsights counters
* **log** AppInsights logger
* **messaging** Azure Storage and Service Bus queues

### Bug Fixes
* Updated documentation
* Added checks for open state in queues
* Added dynamic creation of temporary subscriptions for Service Bus topics


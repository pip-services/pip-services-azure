using PipServices.Azure.Lock;
using PipServices.Commons.Config;

namespace PipServices.Azure.Persistence.Lock
{
    public class CosmosDbThroughputMonitorLock : CloudStorageTableLock, ICosmosDbThroughputMonitorLock
    {
        protected virtual string Key { get; }
        protected virtual long DefaultTimeToLive { get; }

        protected long _timeToLive;

        public override void Configure(ConfigParams config)
        {
            base.Configure(config);

            _timeToLive = config.GetAsNullableInteger("time_to_live") ?? DefaultTimeToLive;
        }

        public bool TryAcquireLock(string correlationId)
        {
            return base.TryAcquireLock(correlationId, Key, _timeToLive);
        }

        public void ReleaseLock(string correlationId)
        {
            base.ReleaseLock(correlationId, Key);
        }
    }

}

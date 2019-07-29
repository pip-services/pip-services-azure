namespace PipServices.Azure.Persistence.Lock
{
    public interface ICosmosDbThroughputMonitorLock
    {
        void ReleaseLock(string correlationId);
        bool TryAcquireLock(string correlationId);
    }
}

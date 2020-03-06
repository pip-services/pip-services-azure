using MongoDB.Driver;

using PipServices.Azure.Metrics.Data;
using PipServices.Azure.Metrics;
using PipServices.Commons.Config;
using PipServices.Commons.Lock;
using PipServices.Commons.Log;
using PipServices.Commons.Refer;
using PipServices.Commons.Run;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PipServices.Azure.Persistence
{
    public abstract class AbstractCosmosDbPersistenceThroughputMonitor : IReferenceable, IConfigurable, IOpenable
    {
        public const int DefaultMinimumThroughput = 400;
        public const int DefaultMaximumThroughput = 100000;
        public const double DefaultGrowthRate = 1.0;

        protected abstract string CorrelationId { get; }

        protected static readonly int DefaultTimerInterval = 1000 * 60 * 5;   // in milliseconds
        protected static readonly int DefaultDelayInterval = 1000 * 60;       // in milliseconds

        protected CompositeLogger _logger = new CompositeLogger();
        protected FixedRateTimer _timer;

        protected ICosmosDbMetricsService _metricsService;
        protected ILock _lock;

        protected bool Enabled { get; set; }
        protected int TimerInterval { get; set; }
        protected int DelayInterval { get; set; }

        protected int MinimumThroughput { get; set; }
        protected int MaximumThroughput { get; set; }
        protected double GrowthRate { get; set; }

        protected string ConnectionUri { get; set; }
        protected string ResourceGroup { get; set; }
        protected string AccountName { get; set; }
        protected string AccessKey { get; set; }
        protected string DatabaseName { get; set; }
        protected abstract string[] CollectionNames { get; }
        protected string InternalCollectionNames => string.Join(",", CollectionNames);

        private ConfigParams _configParams;

        public virtual void Configure(ConfigParams config)
        {
            _configParams = config;

            Enabled = config.GetAsBooleanWithDefault("parameters.enabled", true);
            TimerInterval = config.GetAsNullableInteger("parameters.timer_interval") ?? DefaultTimerInterval;
            DelayInterval = config.GetAsNullableInteger("parameters.delay_interval") ?? DefaultDelayInterval;

            MinimumThroughput = config.GetAsNullableInteger("parameters.minimum_throughput") ?? DefaultMinimumThroughput;
            MaximumThroughput = config.GetAsNullableInteger("parameters.maximum_throughput") ?? DefaultMaximumThroughput;
            GrowthRate = config.GetAsNullableDouble("parameters.growth_rate") ?? DefaultGrowthRate;

            ResourceGroup = config.GetAsString("parameters.resource_group");
            ConnectionUri = config.GetAsString("parameters.connection_uri");
        }

        public virtual void SetReferences(IReferences references)
        {
            try
            {
                _logger.SetReferences(references);

                var mongoDbConnectionUrl = new MongoUrlBuilder(ConnectionUri);
                AccountName = mongoDbConnectionUrl.Username;
                AccessKey = mongoDbConnectionUrl.Password;
                DatabaseName = mongoDbConnectionUrl.DatabaseName;

                _metricsService = references.GetOneRequired<ICosmosDbMetricsService>(new Descriptor("pip-services", "metrics-service", "*", "*", "*"));
                _lock = references.GetOneRequired<ILock>(new Descriptor("pip-services", "lock", "*", "*", "*"));

                _timer = new FixedRateTimer(PerformMonitorAsync, TimerInterval, DelayInterval);
            }
            catch (Exception exception)
            {
                _logger.Error("AbstractCosmosDbPersistenceThroughputMonitor", exception,
                    $"Failed to configure the CosmosDb persistence throughput monitor with parameters '{_configParams}'.");
            }
        }

        public async virtual void PerformMonitorAsync()
        {
            // 1. Check lock - if it's locked, don't process tracking records
            if (!_lock.TryAcquireLock(CorrelationId, $"ThroughputMonitor ({InternalCollectionNames})", TimerInterval))
            {
                _logger.Warn(CorrelationId, $"PerformMonitorAsync: Skip to monitor throughput of '{InternalCollectionNames}' collection(s) (monitoring is locked). The next monitoring is scheduled to {DateTime.UtcNow.AddMilliseconds(TimerInterval).ToString()} UTC.");
                return;
            }

            // 2. Stop timer
            _timer?.Stop();

            _logger.Trace(CorrelationId, $"PerformMonitorAsync: Wake-up to monitor throughput of '{InternalCollectionNames}' collection(s).");

            foreach (var collectionName in CollectionNames)
            {
                // 3. Get throughput metrics for each collection
                var resourceUri = _metricsService.GetResourceUri(CorrelationId, ResourceGroup, AccountName, AccessKey, DatabaseName, collectionName);
                var throughputMetrics = await _metricsService.GetResourceMetricsAsync(CorrelationId, resourceUri,
                        oDataQueryBuilder => oDataQueryBuilder
                            .AddTime(DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(8)), DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(4)), MetricInterval.PT1M)
                            .And()
                            .AddMetric(CosmosDBMetric.Max_RUs_Per_Second));

                // 4. Update throughput if needed
                await PerformUpdateThroughputAsync(CorrelationId, collectionName, throughputMetrics);
            }

            _logger.Trace(CorrelationId, $"PerformMonitorAsync: Completed to monitor throughput of '{InternalCollectionNames}' collection(s). The next monitoring is scheduled to {DateTime.UtcNow.AddMilliseconds(TimerInterval).ToString()} UTC.");

            // 5. Restart timer
            _timer?.Restart();
        }

        public bool IsOpened()
        {
            return _timer != null && _timer.IsStarted;
        }

        public Task OpenAsync(string correlationId)
        {
            if (Enabled)
            {
                _timer?.Start();
                _logger.Info(correlationId, $"OpenAsync: ThroughputMonitor is started for '{InternalCollectionNames}' collection(s).");
            }
            else
            {
                _logger.Info(correlationId, $"OpenAsync: ThroughputMonitor is disabled for '{InternalCollectionNames}' collection(s).");
            }

            return Task.Delay(0);
        }

        public Task CloseAsync(string correlationId)
        {
            if (_timer != null && _timer.IsStarted)
            {
                _timer.Stop();
            }

            return Task.Delay(0);
        }

        private async Task PerformUpdateThroughputAsync(string correlationId, string collectionName, IEnumerable<Metric> throughputMetrics)
        {
            if (throughputMetrics == null || !throughputMetrics.Any())
            {
                _logger.Warn(correlationId, $"PerformUpdateThroughputAsync: Skip to update throughput of collection '{collectionName}', because of missing throughput metrics.");
                return;
            }

            // 1. Group throughput metrics by name
            var groupedMetricsData = throughputMetrics.GroupBy(metric => metric.Name.Value);

            var groupedMetrics = groupedMetricsData.FirstOrDefault();
            if (groupedMetrics == null)
            {
                _logger.Warn(correlationId, $"PerformUpdateThroughputAsync: Unable to group throughput metrics for collection '{collectionName}'.");
                return;
            }

            // 2. Retrieve maximum Request Unit value and partition count
            var partitionCount = groupedMetrics.Count();
            var maximumRequestUnitValue = 0.0;

            foreach (var metric in groupedMetrics)
            {
                if (metric.MetricValues == null)
                {
                    continue;
                }

                maximumRequestUnitValue = Math.Max(maximumRequestUnitValue, metric.MetricValues.Max(metricValue => metricValue.Maximum) ?? 0);
            }

            var recommendedThroughput = CosmosDbThroughputHelper.GetRecommendedThroughput(partitionCount, maximumRequestUnitValue, MinimumThroughput, MaximumThroughput, GrowthRate);
            _logger.Info(correlationId, $"PerformUpdateThroughputAsync for collection '{collectionName}': recommended throughput '{recommendedThroughput}' " +
                $"based on '{groupedMetrics.Key}': '{maximumRequestUnitValue}' " +
                $"with partition count '{partitionCount}' and throughput range '[{MinimumThroughput}, {MaximumThroughput}]' and a growth rate of '{GrowthRate}'.");

            // 3. Perform updating throughput of CosmosDB collection by recommended value
            await PerformUpdateThroughputAsync(correlationId, collectionName, recommendedThroughput);
        }

        protected abstract Task PerformUpdateThroughputAsync(string correlationId, string collectionName, int throughput);
    }
}

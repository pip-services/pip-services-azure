using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using PipServices.Commons.Auth;
using PipServices.Commons.Config;
using PipServices.Commons.Count;
using PipServices.Commons.Refer;
using System.Collections.Generic;

namespace PipServices.Azure.Count
{
    public class AppInsightsCounters : CachedCounters, IDescriptable
    {
        private string _name;
        private CredentialResolver _credentialResolver = new CredentialResolver();
        private TelemetryClient _client;

        public AppInsightsCounters() { }

        public virtual Descriptor GetDescriptor()
        {
            var name = _name ?? "default";
            return new Descriptor("pip-services-azure", "counters", "app-insights", name, "1.0");
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public override void Configure(ConfigParams config)
        {
            base.Configure(config);
            _credentialResolver.Configure(config, true);
            _name = NameResolver.Resolve(config, _name);
        }

        private void Open()
        {
            var credential = _credentialResolver.LookupAsync("count").Result;

            var key = credential.AccessKey 
                ?? credential.GetAsNullableString("instrumentation_key")
                 ?? credential.GetAsNullableString("InstrumentationKey");

            if (key != null)
                TelemetryConfiguration.Active.InstrumentationKey = key;

            _client = new TelemetryClient();
        }

        protected override void Save(IEnumerable<Counter> counters)
        {
            if (_client == null) Open();

            foreach (var counter in counters)
            {
                switch (counter.Type)
                {
                    case CounterType.Increment:
                        _client.TrackMetric(counter.Name, counter.Count.Value);
                        break;
                    case CounterType.Interval:
                        _client.TrackMetric(counter.Name, counter.Average.Value);
                        break;
                    case CounterType.LastValue:
                        _client.TrackMetric(counter.Name, counter.Last.Value);
                        break;
                    case CounterType.Statistics:
                        _client.TrackMetric(counter.Name, counter.Average.Value);
                        break;
                }
            }

            _client.Flush();
        }
    }
}

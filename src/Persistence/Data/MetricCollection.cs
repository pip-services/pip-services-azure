using System.Collections.Generic;

namespace PipServices.Azure.Persistence.Data
{
    public class MetricCollection
    {
        public IList<Metric> Value { get; set; } = new List<Metric>();
    }
}

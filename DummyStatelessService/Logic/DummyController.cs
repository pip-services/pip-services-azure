using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DummyStatelessService.Model;
using PipServices.Commons.Config;
using PipServices.Commons.Log;
using PipServices.Commons.Refer;
using PipServices.Commons.Run;

namespace DummyStatelessService.Logic
{
    public sealed class DummyController : IReferenceable, IReconfigurable, IOpenable, INotifiable,
        IDummyController, IDescriptable
    {
        public static Descriptor Descriptor { get; } = new Descriptor("pip-services-dummies", "controller", "azure", "default", "1.0");

        private readonly FixedRateTimer _timer;
        private readonly CompositeLogger _logger = new CompositeLogger();
        private IList<DummyObject> _persistence;
        public string Message { get; private set; } = "Hello from controller!";
        public long Counter { get; private set; }

        public DummyController()
        {
            _timer = new FixedRateTimer(async () => { await NotifyAsync(null, new Parameters()); }, 1000, 1000);
        }

        public void Configure(ConfigParams config)
        {
            Message = config.GetAsStringWithDefault("message", Message);
        }

        public void SetReferences(IReferences references)
        {
            _logger.SetReferences(references);

            _persistence = new List<DummyObject>();
        }

        public bool IsOpened()
        {
            return _timer.IsStarted;
        }

        public Task OpenAsync(string correlationId)
        {
            _timer.Start();
            _logger.Trace(correlationId, "Dummy controller opened");

            return Task.Delay(0);
        }

        public Task CloseAsync(string correlationId)
        {
            _timer.Stop();

            _logger.Trace(correlationId, "Dummy controller closed");

            return Task.Delay(0);
        }

        public Task NotifyAsync(string correlationId, Parameters args)
        {
            _logger.Info(correlationId, "{0} - {1}", Counter++, Message);

            return Task.Delay(0);
        }

        public Task<DummyObject> CreateAsync(string correlationId, DummyObject entity)
        {
            _logger.Trace(correlationId, "Dummy controller's CreateAsync was called");

            _persistence.Add(entity);

            return Task.FromResult(entity);
        }

        public Task<DummyObject> UpdateAsync(string correlationId, DummyObject entity)
        {
            _logger.Trace(correlationId, "Dummy controller's UpdateAsync was called");

            var ent = _persistence.FirstOrDefault(x => x.Id == entity.Id);

            if (ent != null)
                _persistence.Remove(ent);

            _persistence.Add(entity);

            return Task.FromResult(entity);
        }

        public Task<DummyObject> DeleteByIdAsync(string correlationId, string id)
        {
            _logger.Trace(correlationId, "Dummy controller's DeleteByIdAsync was called");

            var ent = _persistence.FirstOrDefault(x => x.Id == id);

            if (ent != null)
                _persistence.Remove(ent);

            return Task.FromResult(ent);
        }

        public Task<DummyObject> GetOneByIdAsync(string correlationId, string id)
        {
            _logger.Trace(correlationId, "Dummy controller's GetOneByIdAsync was called");

            return Task.FromResult(_persistence.FirstOrDefault(x => x.Id == id));
        }

        public Task<List<DummyObject>> GetListByQueryAsync(string correlationId)
        {
            _logger.Trace(correlationId, "Dummy controller's GetListByQueryAsync was called");

            return Task.FromResult(_persistence.ToList());
        }

        public Descriptor GetDescriptor()
        {
            return Descriptor;
        }
    }
}
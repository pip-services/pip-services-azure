using System.Threading;
using System.Threading.Tasks;
using PipServices.Azure.Run;
using PipServices.Commons.Refer;

namespace DummyStatelessService.Build
{
    internal sealed class DummyServiceContainer : MicroserviceProcessContainer
    {
        private const string CorrelationId = "dummy";

        protected override void InitReferences(IReferences references)
        {
            base.InitReferences(references);

            // Factory to statically resolve dummy components
            references.Put(new DummyStatelessServiceFactory());
        }

        public Task RunAsync(IDescriptable service, CancellationToken token)
        {
            return RunWithConfigFileAsync(CorrelationId, service, "dummy.yaml", token);
        }

        public Task StopAsync(CancellationToken token)
        {
            return StopAsync(CorrelationId, token);
        }
    }
}

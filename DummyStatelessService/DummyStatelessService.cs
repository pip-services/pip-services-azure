using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using DummyStatelessService.Logic;
using DummyStatelessService.Model;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using PipServices.Azure.Run;
using PipServices.Commons.Config;
using PipServices.Commons.Refer;

namespace DummyStatelessService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class DummyStatelessService : MicroserviceStatelessService<IDummyController>, IDummyMicroservice
    {
        /// <summary>
        /// Gets the descriptor.
        /// </summary>
        /// <value>The descriptor.</value>
        public static Descriptor Descriptor { get; } = new Descriptor("pip-services-dummies", "service", "azure", "default", "1.0");

        public DummyStatelessService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(this.CreateServiceRemotingListener)
            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        public override Descriptor GetDescriptor()
        {
            return Descriptor;
        }

        public override void Configure(ConfigParams config)
        {
        }

        public Task<DummyObject> CreateAsync(string correlationId, DummyObject entity)
        {
            return Controller.CreateAsync(correlationId, entity);
        }

        public Task<DummyObject> UpdateAsync(string correlationId, DummyObject entity)
        {
            return Controller.UpdateAsync(correlationId, entity);
        }

        public Task<DummyObject> DeleteByIdAsync(string correlationId, string id)
        {
            return Controller.DeleteByIdAsync(correlationId, id);
        }

        public Task<DummyObject> GetOneByIdAsync(string correlationId, string id)
        {
            return Controller.GetOneByIdAsync(correlationId, id);
        }

        public Task<List<DummyObject>> GetListByQueryAsync(string correlationId)
        {
            return Controller.GetListByQueryAsync(correlationId);
        }
    }
}

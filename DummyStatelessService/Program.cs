using System;
using System.Diagnostics;
using System.Threading;
using DummyStatelessService.Build;
using Microsoft.ServiceFabric.Services.Runtime;

namespace DummyStatelessService
{
    internal static class Program
    {
        private static DummyStatelessService _service;
        private static DummyServiceContainer _processContainer;

        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                _processContainer = new DummyServiceContainer();

                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                ServiceRuntime.RegisterServiceAsync("DummyStatelessServiceType",
                    context =>
                    {
                        _service = new DummyStatelessService(context);

                        _processContainer.RunAsync(_service, CancellationToken.None).Wait();

                        return _service;
                    }
                    ).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(DummyStatelessService).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                _processContainer.StopAsync(CancellationToken.None).Wait();

                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());

                throw;
            }
        }
    }
}

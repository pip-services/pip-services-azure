using System;
using System.Diagnostics;
using System.Threading;
using DummyStatelessService.Build;
using Microsoft.ServiceFabric.Services.Runtime;
using PipServices.Azure.Run;

namespace DummyStatelessService
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            MicroserviceProcessContainer processContainer = null;

            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                ServiceRuntime.RegisterServiceAsync("DummyStatelessServiceType",
                    context =>
                    {
                        processContainer = new MicroserviceProcessContainer();

                        var refer = processContainer.References;
                        
                        refer.Put(context, DummyStatelessServiceFactory.ContextDescriptor);

                        processContainer.RunWithConfigFileAsync("dummy", "dummy.yaml", CancellationToken.None).Wait();

                        var service = processContainer.GetService<DummyStatelessService>();

                        return service;
                    }
                    ).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(DummyStatelessService).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                processContainer?.StopAsync(CancellationToken.None).Wait();

                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());

                throw;
            }
        }
    }
}

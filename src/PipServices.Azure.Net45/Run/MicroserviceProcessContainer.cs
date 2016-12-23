using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using PipServices.Commons.Errors;
using PipServices.Commons.Log;
using PipServices.Commons.Refer;
using PipServices.Commons.Run;
using PipServices.Container.Config;
using PipServices.Container.Info;
using PipServices.Container.Refer;

namespace PipServices.Azure.Run
{
    /// <summary>
    /// Class MicroserviceProcessContainer.
    /// </summary>
    public abstract class MicroserviceProcessContainer : Container.Container
    {
        private string _correlationId;

        private void CaptureErrors()
        {
            AppDomain.CurrentDomain.UnhandledException += HandleUncaughtException;
        }

        private void HandleUncaughtException(object sender, UnhandledExceptionEventArgs args)
        {
            Logger.Fatal(_correlationId, (Exception)args.ExceptionObject, "Process is terminated");
        }

        private void CaptureExit(string correlationId)
        {
            //Logger.Info(null, "Press Control-C to stop the microservice...");

            //AppDomain.CurrentDomain.DomainUnload += (sender, args) => InvokeBatchProcessors(correlationId, Logger, _exitEvent);

            //// Wait and close
            //try
            //{
            //    _exitEvent.Wait();
            //}
            //catch (OperationCanceledException)
            //{
            //    // Ignore...
            //}
            //catch (ObjectDisposedException)
            //{
            //    // Ignore...
            //}
        }

        private void InvokeBatchProcessors(string correlationId, ILogger logger, SemaphoreSlim exitEvent)
        {
            logger.Info(correlationId, "Goodbye!");
        }

        private async Task RunAsync(string correlationId, IDescriptable service, CancellationToken token)
        {
            _correlationId = correlationId;

            CaptureErrors();

            await StartAsync(correlationId, service, token);
        }

        public Task RunWithConfigFileAsync(string correlationId, IDescriptable service, string path, CancellationToken token)
        {
            ReadConfigFromFile(correlationId, path);
            return RunAsync(correlationId, service, token);
        }

        private async Task StartAsync(string correlationId, IDescriptable service, CancellationToken token)
        {
            if (Config == null)
                throw new InvalidStateException(correlationId, "NO_CONFIG", "Container was not configured");

            try
            {
                Logger.Trace(correlationId, "Starting container.");

                // Create references with configured components
                References = new ContainerReferences();
                InitReferences(References);
                References.PutFromConfig(Config);

                References.Put(service, service.GetDescriptor());

                // Reference and open components
                var components = References.GetAll();
                Referencer.SetReferencesForComponents(References, components);
                await Opener.OpenComponentsAsync(correlationId, References.GetAll());

                // Get reference to logger
                Logger = new CompositeLogger(References);

                // Get reference to container info
                var infoDescriptor = new Descriptor("*", "container-info", "*", "*", "*");
                Info = (ContainerInfo)References.GetOneRequired(infoDescriptor);

                Logger.Info(correlationId, "Container {0} started.", Info.Name);
            }
            catch (Exception ex)
            {
                References = null;
                Logger.Error(correlationId, ex, "Failed to start container");

                throw;
            }
        }
    }
}

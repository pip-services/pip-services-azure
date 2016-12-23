using System.Fabric;
using DummyStatelessService.Logic;
using PipServices.Commons.Build;
using PipServices.Commons.Refer;

namespace DummyStatelessService.Build
{
    /// <summary>
    /// Class DummyStatelessServiceFactory. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="PipServices.Commons.Build.IFactory" />
    /// <seealso cref="PipServices.Commons.Refer.IDescriptable" />
    internal sealed class DummyStatelessServiceFactory : IFactory, IDescriptable
    {
        private IDummyController Controller { get; set; }

        public static Descriptor Descriptor { get; } = new Descriptor("pip-services-dummies", "factory", "azure", "default", "1.0");

        public bool CanCreate(object locater)
        {
            var descriptor = locater as Descriptor;

            if (descriptor == null)
                return false;

            if (descriptor.Match(DummyController.Descriptor))
                return true;

            return false;
        }

        public object Create(object locater)
        {
            var descriptor = locater as Descriptor;

            if (descriptor == null)
                return null;

            if (descriptor.Match(DummyController.Descriptor))
            {
                Controller = Controller ?? new DummyController();
                return Controller;
            }

            return null;
        }

        public Descriptor GetDescriptor()
        {
            return Descriptor;
        }
    }
}

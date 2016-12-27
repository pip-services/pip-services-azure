using System;
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
    internal sealed class DummyStatelessServiceFactory : IFactory, IDescriptable, IReferenceable
    {
        private IReferences _references;

        public static Descriptor Descriptor { get; } = new Descriptor("pip-services-dummies", "factory", "azure", "default", "1.0");

        public static Descriptor ContextDescriptor { get; } = new Descriptor("pip-services-dummies", "context", "azure", "stateless", "1.0");

        /// <summary>
        /// Determines whether this instance can create the specified locater.
        /// </summary>
        /// <param name="locater">The locater.</param>
        /// <returns><c>true</c> if this instance can create the specified locater; otherwise, <c>false</c>.</returns>
        public bool CanCreate(object locater)
        {
            var descriptor = locater as Descriptor;

            if (descriptor == null)
                return false;

            if (descriptor.Match(DummyController.Descriptor))
                return true;

            if (descriptor.Match(DummyStatelessService.Descriptor))
                return true;

            return false;
        }

        /// <summary>
        /// Creates the specified locater.
        /// </summary>
        /// <param name="locater">The locater.</param>
        /// <returns>System.Object.</returns>
        public object Create(object locater)
        {
            var descriptor = locater as Descriptor;

            if (descriptor == null)
                return null;

            if (descriptor.Match(DummyController.Descriptor))
                return new DummyController();

            if (descriptor.Match(DummyStatelessService.Descriptor))
            {
                var context = _references.GetOneRequired<StatelessServiceContext>(ContextDescriptor);

                return new DummyStatelessService(context);
            }

            return null;
        }

        /// <summary>
        /// Gets the descriptor.
        /// </summary>
        /// <returns>Descriptor.</returns>
        public Descriptor GetDescriptor()
        {
            return Descriptor;
        }

        public void SetReferences(IReferences references)
        {
            _references = references;
        }
    }
}

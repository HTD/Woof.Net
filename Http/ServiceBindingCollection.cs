using System.Collections;
using System.Collections.Generic;

namespace Woof.Net.Http {

    /// <summary>
    /// Represents a specialized collection of <see cref="ServiceBinding"/> items.
    /// </summary>
    public class ServiceBindingCollection : IEnumerable<ServiceBinding> {

        /// <summary>
        /// Service bindings.
        /// </summary>
        public readonly List<ServiceBinding> Items = new List<ServiceBinding>();

        /// <summary>
        /// Creates an empty service binding collection.
        /// </summary>
        public ServiceBindingCollection() { }

        /// <summary>
        /// Creates a service binding collection from any other service binding collection.
        /// </summary>
        /// <param name="serviceBindings">Service binding collection.</param>
        public ServiceBindingCollection(IEnumerable<ServiceBinding> serviceBindings) => Items.AddRange(serviceBindings);

        /// <summary>
        /// Adds a new binding to internal items list.
        /// </summary>
        /// <param name="binding">Service binding.</param>
        public void Add(ServiceBinding binding) => Items.Add(binding);

        /// <summary>
        /// Adds a new service binding from the service type alone.
        /// The instance of the service class will be created on each request.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        public void Add<T>() where T: class, new() => Items.Add(new ServiceBinding(typeof(T)));

        /// <summary>
        /// Adds a new service binding from the service type alone.
        /// The instance of the service class will be created on each request.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <param name="pathPrefix">The first relative URL part used to match the requests.</param>
        public void Add<T>(string pathPrefix) where T : class, new() => Items.Add(new ServiceBinding(typeof(T)) { PathPrefix = pathPrefix });

        /// <summary>
        /// Add a new service binding from the singleton instance of the service.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <param name="instance">Service signleton instance.</param>
        public void Add<T>(T instance) where T : class, new() => Items.Add(new ServiceBinding(instance));

        /// <summary>
        /// Add a new service binding from the singleton instance of the service.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <param name="instance">Service signleton instance.</param>
        /// <param name="pathPrefix">The first relative URL part used to match the requests.</param>
        public void Add<T>(T instance, string pathPrefix) where T : class, new() => Items.Add(new ServiceBinding(instance) { PathPrefix = pathPrefix });

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ServiceBinding> GetEnumerator() => ((IEnumerable<ServiceBinding>)Items).GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ServiceBinding>)Items).GetEnumerator();

    }

}
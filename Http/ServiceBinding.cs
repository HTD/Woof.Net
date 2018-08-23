using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;

namespace Woof.Net.Http {

    /// <summary>
    /// Defines a binding between path prefix and a service contract.
    /// Allows hosting web services with <see cref="Server"/>.
    /// </summary>
    public class ServiceBinding {

        /// <summary>
        /// Gets or sets the first relative URL part used to match the requests.
        /// </summary>
        public string PathPrefix { get; set; }

        /// <summary>
        /// Gets or sets the serializer used to serialize and deserialize data from and to operation contracts.
        /// </summary>
        public IContractSerializer ContractSerializer {
            get =>
#if DEBUG
                _ContractSerializer ?? (_ContractSerializer = new JsonContractSerializer(new JsonSerializerSettings { Formatting = Formatting.Indented }));
#else
                _ContractSerializer ?? (_ContractSerializer = new JsonContractSerializer());
#endif
            set => _ContractSerializer = value;
        }

        /// <summary>
        /// Processes incoming HTTP request from the server.
        /// Uses a matched operation contract to provide response.
        /// Returns true if the request was handled here.
        /// </summary>
        /// <param name="context">HTTP server context.</param>
        /// <returns>True if the request was handled, false otherwise.</returns>
        internal bool ProcessHttpRequest(ServerContext context) {
            var instance = ServiceInstance ?? Activator.CreateInstance(ServiceType);
            if (String.IsNullOrEmpty(PathPrefix)) {
                foreach (var operationContract in OperationContracts)
                    if (operationContract.ProcessHttpRequest(context, instance, context.RequestPath.Trim('/'), ContractSerializer))
                        return true;
            } else {
                if (context.TryResolveLocalPrefix(PathPrefix, out var relativeUrl)) {
                    foreach (var operationContract in OperationContracts)
                        if (operationContract.ProcessHttpRequest(context, instance, relativeUrl.Trim('/'), ContractSerializer))
                            return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates a service binding from the service type alone.
        /// The instance of the service class will be created on each request.
        /// </summary>
        /// <param name="serviceType">Service type.</param>
        public ServiceBinding(Type serviceType) : this(serviceType, null) { }

        /// <summary>
        /// Creates a service binding from the singleton instance of the service.
        /// </summary>
        /// <param name="instance">Service instance.</param>
        public ServiceBinding(object instance) : this(null, instance) { }

        /// <summary>
        /// Creates a service binding from a service type or a service instance.
        /// If the instance is given, the type is ignored.
        /// If the type is given, the service class is instantiated on each request.
        /// </summary>
        /// <param name="serviceType">Service type.</param>
        /// <param name="instance">Service instance.</param>
        private ServiceBinding(Type serviceType, object instance) {
            ServiceType = instance?.GetType() ?? serviceType;
            if (ServiceType == null) throw new NullReferenceException();
            if (!ServiceType.IsClass) throw new InvalidOperationException("Service type must be a class");
            ServiceInstance = instance;
            var interfaceType = ServiceType.GetInterfaces().FirstOrDefault(i => i.GetCustomAttribute<ServiceContractAttribute>() != null);
            var serviceContract =
                ServiceType.GetCustomAttribute<ServiceContractAttribute>()
                ?? interfaceType.GetCustomAttribute<ServiceContractAttribute>();
            var methods = ServiceType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var interfaceMethods = interfaceType?.GetMethods() ?? new MethodInfo[0];
            var operationContractsFromInterfaceMethods =
                interfaceMethods
                    .Where(i => Attribute.IsDefined(i, typeof(OperationContractAttribute), true))
                    .Select(i => new OperationContract(i, i.GetCustomAttribute<OperationContractAttribute>()));
            var operationContractsFromImplementationMethods =
                methods
                    .Where(i => Attribute.IsDefined(i, typeof(OperationContractAttribute), true))
                    .Select(i => new OperationContract(i, i.GetCustomAttribute<OperationContractAttribute>()));
            OperationContracts = operationContractsFromInterfaceMethods.Union(operationContractsFromImplementationMethods).ToArray();
        }

        /// <summary>
        /// Discovered operation contracts.
        /// </summary>
        public readonly OperationContract[] OperationContracts;
        
        /// <summary>
        /// Service signleton instance if available, null when the service class has to be instantiated on each request.
        /// </summary>
        private readonly object ServiceInstance;

        /// <summary>
        /// Service type.
        /// </summary>
        private readonly Type ServiceType;

        /// <summary>
        /// Contract serializer instance cache.
        /// </summary>
        private IContractSerializer _ContractSerializer;

    }

}
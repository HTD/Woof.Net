using System;

namespace Woof.Net.Http {

    /// <summary>
    /// Attribute providing metadata to service contract interface or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true)]
    public class ServiceContractAttribute : Attribute { }

    /// <summary>
    /// Attribute providing metadata to an operation contract method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class OperationContractAttribute : Attribute {

        /// <summary>
        /// Gets the contract HTTP method like "get" or "post".
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// Gets the URI pattern to match in the relative path, like "method/{param1}"
        /// </summary>
        public string UriPattern { get; set; }

        /// <summary>
        /// Gets the response content type for binary streams.
        /// For data objects this has no effect, because serializer content type is used.
        /// </summary>
        public string ReturnContentType { get; set; }

        /// <summary>
        /// Creates new <see cref="OperationContractAttribute"/> from method and optional URI pattern.
        /// </summary>
        /// <param name="httpMethod">HTTP (upper case) method name like "GET" or "POST".</param>
        /// <param name="uriPattern">URI pattern like "method/{param1}"</param>
        /// <param name="returnContentType">Response MIME type for binary streams.</param>
        public OperationContractAttribute(string httpMethod = "GET", string uriPattern = null, string returnContentType = null) {
            HttpMethod = httpMethod.ToUpperInvariant();
            UriPattern = uriPattern;
            ReturnContentType = returnContentType;
        }

    }

}
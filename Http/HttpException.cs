using System;
using System.Net;

namespace Woof.Net.Http {

    /// <summary>
    /// Represents HTTP server fault exception.
    /// </summary>
    public class HttpException : Exception {

        /// <summary>
        /// Gets HTTP status code.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; }

        /// <summary>
        /// Gets the status decription if available.
        /// </summary>
        public string HttpStatusDescription { get; }

        /// <summary>
        /// Creates HTTP server fault exception.
        /// </summary>
        /// <param name="httpStatusCode">HTTP status code.</param>
        /// <param name="httpStatusCodeDescription">Optional status description.</param>
        public HttpException(HttpStatusCode httpStatusCode, string httpStatusCodeDescription = null) {
            HttpStatusCode = httpStatusCode;
            HttpStatusDescription = httpStatusCodeDescription ?? httpStatusCode.ToString();
        }

    }

}
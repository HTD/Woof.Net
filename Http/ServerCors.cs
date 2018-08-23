using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Woof.Net.Http {

    /// <summary>
    /// Handles Cross-Origin Resource Sharing.
    /// </summary>
    public partial class Server {

        /// <summary>
        /// Gets or sets a value indicating whether the server can accept cross-domain cookies.
        /// </summary>
        public bool AccessControlAllowCredentials { get; set; } = true;

        /// <summary>
        /// Gets or sets a value containing headers allowed for cross-domain requests. The separator is ", ".
        /// "*" means all requested headers.
        /// </summary>
        public string AccessControlAllowHeaders { get; set; } = "*";

        /// <summary>
        /// Gets or sets a value containing methods allowed for cross-domain requests. The separator is ", ".
        /// "*" means all requested methods.
        /// </summary>
        public string AccessControlAllowMethods { get; set; } = "*";

        /// <summary>
        /// Gets or sets a value indicating how long the results of a preflight request (that is the information contained in the Access-Control-Allow-Methods and Access-Control-Allow-Headers headers) can be cached.
        /// </summary>
        public string AccessControlMaxAge { get; set; }

        /// <summary>
        /// Gets or sets a value containing one or more domains allowed to send requests.
        /// The separator is ", ". Use "*" to allow all origins.
        /// </summary>
        public string AccessControlAllowOrigin { get; set; }

        /// <summary>
        /// Handles CORS requests.
        /// </summary>
        /// <param name="context">Server context.</param>
        /// <returns>True if request is allowed, false otherwise.</returns>
        private bool CorsProcess(ServerContext context) {
            var originAllowed = CorsGetOriginAllowed(context);
            if (originAllowed == null) {
                if (context.Request.Url.OriginalString.StartsWith(context.Request.Headers["Origin"], StringComparison.OrdinalIgnoreCase))
                    return true;
                    else return CorsDeny(context);
            }
            else return CorsAllow(context, originAllowed);
        }

        /// <summary>
        /// Gets the one origin from defined list if matches. Returns null if the origin should not be allowed.
        /// </summary>
        /// <param name="context">Server context.</param>
        /// <returns>*, requestOrigin or null.</returns>
        private string CorsGetOriginAllowed(ServerContext context) {
            if (String.IsNullOrEmpty(AccessControlAllowOrigin)) return null;
            if (AccessControlAllowOrigin == "*") return AccessControlAllowOrigin;
            var requestOrigin = context.Request.Headers["Origin"];
            if (!AccessControlAllowOrigin.Contains(',') && requestOrigin.Equals(AccessControlAllowOrigin, StringComparison.OrdinalIgnoreCase) ||
                RxByComaWs.Split(AccessControlAllowOrigin).Any(i => requestOrigin.Equals(i, StringComparison.OrdinalIgnoreCase))) return requestOrigin;
            return null;
        }

        /// <summary>
        /// Accepts CORS simple and preflight requests.
        /// </summary>
        /// <param name="context">Server context.</param>
        /// <param name="originAllowed">Origin allowed.</param>
        /// <returns>Always true.</returns>
        private bool CorsAllow(ServerContext context, string originAllowed) {
            context.Response.AddHeader("Access-Control-Allow-Origin", originAllowed);
            if (context.IsPreflightDetected) {
                context.Response.AddHeader("Access-Control-Allow-Headers",
                    AccessControlAllowHeaders == "*"
                        ? context.Request.Headers["Access-Control-Request-Headers"]
                        : AccessControlAllowHeaders
                );
                context.Response.AddHeader("Access-Control-Allow-Methods",
                    AccessControlAllowMethods == "*"
                        ? context.Request.Headers["Access-Control-Request-Methods"]
                        : AccessControlAllowMethods
                );
            }
            if (AccessControlMaxAge != null) context.Response.AddHeader("Access-Control-Max-Age", AccessControlMaxAge);
            if (AccessControlAllowCredentials) context.Response.AddHeader("Access-Control-Allow-Credentials", "true");
            return true;
        }

        /// <summary>
        /// Denies CORS simple and preflight requests.
        /// </summary>
        /// <param name="context">Server context.</param>
        /// <returns>Always false.</returns>
        private bool CorsDeny(ServerContext context) {
            context.Response.StatusCode = 403;
            context.Response.StatusDescription = "Origin not allowed";
            return false;
        }

        /// <summary>
        /// Regular expression used to split strings by coma with optional whitespace.
        /// </summary>
        private static readonly Regex RxByComaWs = new Regex(@",\s*", RegexOptions.Compiled);

    }

}
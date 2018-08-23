using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;

namespace Woof.Net.Http {

    /// <summary>
    /// Represents HTTP server context containing Path, Request and Response objects.
    /// </summary>
    public class ServerContext {

        /// <summary>
        /// Gets or sets the server context for the request in the current thread.
        /// Adds the context to the static collection if the context is new.
        /// </summary>
        public static ServerContext Current {
            get {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                return PerThreadContexts.ContainsKey(threadId) ? PerThreadContexts[threadId] : null;
            }
            private set {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                if (!PerThreadContexts.TryAdd(threadId, value)) PerThreadContexts[threadId] = value;
            }
        }

        /// <summary>
        /// Gets true if request contains "Origin" header.
        /// </summary>
        public bool IsOriginPresent { get; }

        /// <summary>
        /// Gets true if request contains "Origin" header and the request type is "OPTIONS".
        /// </summary>
        public bool IsPreflightDetected { get; }

        /// <summary>
        /// Gets the URL of the request relative to the matched prefix, or null if the prefix is not matched.
        /// </summary>
        public string RequestPath { get; }

        /// <summary>
        /// Gets the request received.
        /// </summary>
        public HttpListenerRequest Request { get; }

        /// <summary>
        /// Gets the response before sending.
        /// </summary>
        public HttpListenerResponse Response { get; }

        /// <summary>
        /// Creates new HTTP server context from prefixes and listener context.
        /// </summary>
        /// <param name="prefixes">URL prefixes.</param>
        /// <param name="listenerContext">Listener context.</param>
        internal ServerContext(HttpListenerPrefixCollection prefixes, HttpListenerContext listenerContext) {
            Current = this;
            Request = listenerContext.Request;
            Response = listenerContext.Response;
            var prefix = prefixes.First(i => Request.Url.AbsoluteUri.StartsWith(i.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)).Trim('/');
            RequestPath = Request.Url.AbsoluteUri.Substring(prefix.Length);
            IsOriginPresent = Request.Headers["Origin"] != null;
            IsPreflightDetected = IsOriginPresent && Request.HttpMethod == "OPTIONS";
        }

        /// <summary>
        /// Removes the current thread's server context from the static context collection.
        /// </summary>
        internal static void Leave() {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            PerThreadContexts.TryRemove(threadId, out var removedContext);
        }

        /// <summary>
        /// Tries to resolve the relative URL considering the local path prefix.
        /// </summary>
        /// <param name="pathPrefix">Local path prefix.</param>
        /// <param name="relativeUrl">Relative URL without prefix resolved, or null if set and not matched.</param>
        /// <returns>True if the prefix is not set or matched.</returns>
        public bool TryResolveLocalPrefix(string pathPrefix, out string relativeUrl) {
            relativeUrl = null;
            if (String.IsNullOrEmpty(pathPrefix)) {
                relativeUrl = RequestPath;
                return true;
            } else {
                pathPrefix = pathPrefix.Trim('/');
                if (RequestPath.Equals(pathPrefix, StringComparison.OrdinalIgnoreCase)) {
                    relativeUrl = String.Empty;
                    return true;
                }
                if (RequestPath.Equals(pathPrefix + '/', StringComparison.OrdinalIgnoreCase)) {
                    relativeUrl = "/";
                    return true;
                }
                if (RequestPath.Length <= pathPrefix.Length) return false;
                if (!RequestPath.Substring(1, pathPrefix.Length).Equals(pathPrefix)) return false;
                relativeUrl = RequestPath.Substring(pathPrefix.Length + 1);
                return true;
            }
        }

        /// <summary>
        /// Contains server contexts for each parallel thread.
        /// </summary>
        private static readonly ConcurrentDictionary<int, ServerContext> PerThreadContexts = new ConcurrentDictionary<int, ServerContext>();

    }

}
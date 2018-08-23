using System;

namespace Woof.Net.Http {

    /// <summary>
    /// Event arguments for <see cref="Server.Request"/> event.
    /// </summary>
    public class HttpRequestEventArgs : EventArgs {

        /// <summary>
        /// Gets the server context.
        /// </summary>
        public ServerContext Context { get; }

        /// <summary>
        /// Gets or sets a value indicating the request was handled within the event handler.
        /// When true, all further site and service binding will be skipped.
        /// </summary>
        public bool IsHandled { get; set; }

        /// <summary>
        /// Creates new <see cref="HttpRequestEventArgs"/> with a specified server context.
        /// </summary>
        /// <param name="context">Server context.</param>
        public HttpRequestEventArgs(ServerContext context) => Context = context;

    }

}
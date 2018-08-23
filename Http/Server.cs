using System;
using System.Net;
using System.Threading;

namespace Woof.Net.Http {

    // TODO: POST requests handling.
    // TODO: Server variables accesible from HTML.

    /// <summary>
    /// Modern HTTP server.
    /// </summary>
    public partial class Server : IDisposable {

        /// <summary>
        /// Occurs when <see cref="HttpListenerException"/> is thrown within listening loop.
        /// </summary>
        public event EventHandler<HttpListenerException> ListenerException;

        /// <summary>
        /// Occurs when a request is sent to the server.
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> Request;
        
        /// <summary>
        /// Gets or sets the <see cref="HttpListener"/> instance used for HTTP communication.
        /// </summary>
        private HttpListener Listener { get; set; }

        /// <summary>
        /// Gets or sets maximum number of independent request processing threads.
        /// </summary>
        public int MaxConcurrentRequests { get; set; } = 16;

        /// <summary>
        /// Gets the prefixes configured with the constructor and bound to the server.
        /// </summary>
        public string[] Prefixes { get; }

        /// <summary>
        /// Gets the value defining how relative paths within server prefix are mapped with local sites.
        /// </summary>
        public SiteBindingCollection SiteBindings { get; } = new SiteBindingCollection();

        /// <summary>
        /// Gets the bound services collection.
        /// </summary>
        public ServiceBindingCollection ServiceBindings { get; } = new ServiceBindingCollection();

        /// <summary>
        /// Gets the value indicating whether the server has been started.
        /// </summary>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// Gets the value indicating whether the server instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        private bool IsStopping;

        /// <summary>
        /// Creates HTTP server instance for given prefixes.
        /// </summary>
        /// <param name="prefixes">Prefixes to bind with the server.</param>
        public Server(params string[] prefixes) => Prefixes = prefixes;

        /// <summary>
        /// Starts the server.
        /// </summary>
        public void Start() {
            if (IsStarted) return;
            Listener = new HttpListener { IgnoreWriteExceptions = true };
            foreach (var prefix in Prefixes) Listener.Prefixes.Add(prefix.Trim('/') + '/');
            //Listener.TimeoutManager.IdleConnection = TimeSpan.FromSeconds(3600);
            Listener.Start();
            for (int i = 0; i < MaxConcurrentRequests; i++) Listener.BeginGetContext(GetContextCallback, this);
            IsStarted = true;
            IsStopping = false;
        }
        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop() {
            if (!IsStarted) return;
            IsStopping = true;
            Listener.Stop();
            Listener.Close();
            Listener = null;
            IsStarted = false;
        }

        /// <summary>
        /// Disposes the server.
        /// </summary>
        public void Dispose() {
            if (IsStarted) Stop();
            if (Listener != null) ((IDisposable)Listener).Dispose();
            IsDisposed = true;
        }

        /// <summary>
        /// Handles asynchronous HTTP requests.
        /// </summary>
        /// <param name="asyncResult">Status of the asynchronous operation.</param>
        private void GetContextCallback(IAsyncResult asyncResult) {
            if (IsStopping) return;
            var httpListenerContext = Listener.EndGetContext(asyncResult);
            if (Listener.IsListening) Listener.BeginGetContext(GetContextCallback, this);

            var serverContext = new ServerContext(Listener.Prefixes, httpListenerContext);
            
            try {
                if (serverContext.IsOriginPresent) {
                    var isCorsRequestAccepted = CorsProcess(serverContext);
                    if (!isCorsRequestAccepted || serverContext.IsPreflightDetected) {
                        serverContext.Response.Close();
                        return;
                    }
                }
                if (Request != null) {
                    var requestEventArgs = new HttpRequestEventArgs(serverContext);
                    Request.Invoke(this, requestEventArgs);
                    if (requestEventArgs.IsHandled) return;
                }
                foreach (var binding in ServiceBindings) if (binding.ProcessHttpRequest(serverContext)) return;
                foreach (var binding in SiteBindings) if (binding.ProcessHttpRequest(serverContext)) return;
                serverContext.Response.StatusCode = 404;
                serverContext.Response.Close();
            }
            catch (HttpListenerException x) {
                ListenerException?.Invoke(this, x);
            }
            finally {
                ServerContext.Leave();
            }
        }

    }

}
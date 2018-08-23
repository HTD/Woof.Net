using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Text;

namespace Woof.Net.Http {

    /// <summary>
    /// Extensions writing different types of data to the <see cref="HttpListenerResponse.OutputStream"/>.
    /// </summary>
    public static class ListenerResponseExtensions {

        /// <summary>
        /// Sends a JSON object as response. Response stream is closed afterwards.
        /// </summary>
        /// <param name="response">This response.</param>
        /// <param name="data">JSON object.</param>
        public static void SendJson(this HttpListenerResponse response, JObject data)
            => SendText(response, data.ToString(), "application/json");

        /// <summary>
        /// Sends HTML as response. Response stream is closed afterwards.
        /// </summary>
        /// <param name="response">This response.</param>
        /// <param name="html">HTML.</param>
        public static void SendHTML(this HttpListenerResponse response, string html)
            => SendText(response, html, "text/html");

        /// <summary>
        /// Sends some plain text as response. Response stream is closed afterwards.
        /// </summary>
        /// <param name="response">This response.</param>
        /// <param name="text">Text.</param>
        /// <param name="contentType">Optional content type.</param>
        public static void SendText(this HttpListenerResponse response, string text, string contentType = "text/plain") {
            response.ContentEncoding = Encoding.UTF8;
            SendData(response, response.ContentEncoding.GetBytes(text), contentType);
        }

        /// <summary>
        /// Sends some binary data as response. Response stream is closed afterwards.
        /// </summary>
        /// <param name="response">This response.</param>
        /// <param name="data">Binary data.</param>
        /// <param name="contentType">Optional content type.</param>
        public static void SendData(this HttpListenerResponse response, byte[] data, string contentType = null) {
            try {
                response.ContentLength64 = data.LongLength;
                if (contentType != null) response.ContentType = contentType;
                response.OutputStream.Write(data, 0, data.Length);
            }
            finally {
                response.OutputStream.Close();
            }
        }

        /// <summary>
        /// Sends a stream as response. Response stream is closed afterwards.
        /// </summary>
        /// <param name="response">This response.</param>
        /// <param name="stream">Stream to send. The stream is automatically disposed when sent.</param>
        /// <param name="contentType">Content type.</param>
        public static void SendStream(this HttpListenerResponse response, Stream stream, string contentType = null) {
            try {
                if (stream.CanSeek) response.ContentLength64 = stream.Length;
                if (contentType != null) response.ContentType = contentType;
                using (stream) stream.CopyTo(response.OutputStream);
            }
            finally {
                response.OutputStream.Close();
                response.Close();
            }
        }

    }

}
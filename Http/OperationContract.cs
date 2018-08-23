using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace Woof.Net.Http {

    /// <summary>
    /// Defines operation contract with method signature and metadata, provides metods to match contracts to requests and invoke them.
    /// </summary>
    public class OperationContract {

        /// <summary>
        /// Gets the contract's method signature.
        /// </summary>
        public MethodInfo Signature { get; }

        /// <summary>
        /// Gets the contract's metadata.
        /// </summary>
        public OperationContractAttribute Metadata { get; }

        /// <summary>
        /// Creates new operation contract definition from method signature and metadata.
        /// </summary>
        /// <param name="signature">Method signature.</param>
        /// <param name="metadata">Metadata.</param>
        public OperationContract(MethodInfo signature, OperationContractAttribute metadata) {
            Signature = signature;
            Metadata = metadata;
        }

        /// <summary>
        /// Returns true if URI priovided matches the method signature name directly or matches the pattern provided.
        /// </summary>
        /// <param name="signature">Method signature.</param>
        /// <param name="uri">URI to parse.</param>
        /// <param name="pattern">Optional URI pattern.</param>
        /// <returns>True if URI matches, false otherwise.</returns>
        public static bool IsUriMatch(MethodInfo signature, string uri, string pattern = null) {
            if (pattern == null) {
                if (uri == null) return false;
                var q = uri.IndexOf('?');
                if (q > 0) return uri.Substring(0, q).Equals(signature.Name, StringComparison.OrdinalIgnoreCase);
                return uri.Equals(signature.Name, StringComparison.OrdinalIgnoreCase);
            }
            else {
                var patternEscaped = Regex.Escape(pattern);
                var patternMatching = '^' + RxCurlyBracesContent.Replace(patternEscaped, ".*") + '$';
                return Regex.IsMatch(uri, patternMatching);
            }
        }

        /// <summary>
        /// Gets serialized input values as <see cref="NameValueCollection"/>.
        /// </summary>
        /// <param name="uri">URI to parse.</param>
        /// <param name="pattern">Optional URI pattern.</param>
        /// <returns>Serialized values as <see cref="NameValueCollection"/>.</returns>
        public static NameValueCollection GetSerializedValues(string uri, string pattern = null) {
            if (pattern == null) {
                var q = uri.IndexOf('?');
                return HttpUtility.ParseQueryString(WebUtility.UrlDecode(q < 0 ? uri : uri.Substring(q)));
            }
            var patternEscaped = Regex.Escape(pattern);
            var patternMatching = '^' + RxCurlyBracesContent.Replace(patternEscaped, "(.*)") + '$';
            var match = Regex.Match(uri, patternMatching);
            if (match.Success) {
                var templateNames = RxCurlyBracesContent.Matches(patternEscaped).OfType<Match>().Select(i => i.Groups[1].Value).ToArray();
                var serializedValues = match.Groups.OfType<Group>().Skip(1).Select(i => i.Value).ToArray();
                return
                    templateNames
                    .Zip(serializedValues, (n, v) => new KeyValuePair<string, string>(n, v))
                    .Aggregate(new NameValueCollection(), (a, v) => { a.Add(v.Key, v.Value); return a; });
            }
            return new NameValueCollection();
        }

        /// <summary>
        /// Gets deserialized input parameter values.
        /// </summary>
        /// <param name="signature">Method signature.</param>
        /// <param name="serializer">Contract serializer.</param>
        /// <param name="uri">Request URI.</param>
        /// <param name="pattern">Optional URI pattern.</param>
        /// <returns>Deserialized input parameter values.</returns>
        public static object[] GetParameterValues(MethodInfo signature, IContractSerializer serializer, string uri, string pattern = null) {
            var parameters = signature.GetParameters();
            var serializedValues = GetSerializedValues(uri, pattern);
            if (serializedValues.Count < 1) return new object[] { };
            if (parameters.Length == 1) {
                var n = parameters[0].Name;
                var t = parameters[0].ParameterType;
                if (!t.IsPrimitive && t != typeof(string) && t != typeof(decimal) && t != typeof(DateTime) && t != typeof(TimeSpan)) {
                    if (t == typeof(object)) { // one object case
                        var instance = new ExpandoObject() as IDictionary<string, object>;
                        foreach (var key in serializedValues.AllKeys) instance[key] = serializer.Deserialize(serializedValues[key]);
                        return new object[] { instance };
                    }
                    else
                    if (t.IsClass || t.IsInterface) { // one class case
                        var fields = t.GetFields();
                        if (fields.Length == serializedValues.Count) {
                            var instance = Activator.CreateInstance(t);
                            foreach (var field in fields) {
                                field.SetValue(instance, serializer.Deserialize(serializedValues[field.Name], field.FieldType));
                            }
                            return new object[] { instance };
                        }
                        else {
                            var properties = t.GetProperties();
                            if (properties.Length == serializedValues.Count) {
                                var instance = Activator.CreateInstance(t);
                                foreach (var property in properties) {
                                    property.SetValue(instance, serializer.Deserialize(serializedValues[property.Name], property.PropertyType));
                                }
                                return new object[] { instance };
                            }
                        }
                    }
                    else
                    if (t.IsValueType && !t.IsEnum) { // one struct case
                        var fields = t.GetFields();
                        if (fields.Length == serializedValues.Count) {
                            var instance = Activator.CreateInstance(t);
                            foreach (var field in fields) {
                                field.SetValue(instance, serializer.Deserialize(serializedValues[field.Name], field.FieldType));
                            }
                            return new object[] { instance };
                        }
                    }
                    throw new InvalidOperationException("Unsupported type");
                }
                return new object[] { serializer.Deserialize(serializedValues[n], t) };
            }
            return parameters.Select(i => serializer.Deserialize(serializedValues[i.Name], i.ParameterType)).ToArray();
        }

        /// <summary>
        /// Processes incoming HTTP request from the server.
        /// Uses a matched operation contract to provide response.
        /// Returns true if the request was handled here.
        /// </summary>
        /// <param name="context">HTTP server context.</param>
        /// <param name="serviceInstance">An instance of the service class.</param>
        /// <param name="uri">Relative operation contract URI without prefix.</param>
        /// <param name="contractSerializer">A serializer used to serialize and deserialize contract's data.</param>
        /// <returns>True if the request was handled, false otherwise.</returns>
        public bool ProcessHttpRequest(ServerContext context, object serviceInstance, string uri, IContractSerializer contractSerializer) {
            if (!IsUriMatch(Signature, uri, Metadata.UriPattern)) return false;
            object result = null;            
            try {
                switch (Metadata.HttpMethod) {
                    case "GET":
                        result = Signature.Invoke(serviceInstance, GetParameterValues(Signature, contractSerializer, uri, Metadata.UriPattern));
                        break;
                    case "POST":
                        if (context.Request.ContentType.Contains("stream") || context.Request.ContentType.Contains("image")) {
                            var argumentType = Signature.GetParameters().First().ParameterType;
                            if (argumentType.IsAssignableFrom(typeof(Stream))) {
                                result = Signature.Invoke(serviceInstance, new[] { context.Request.InputStream });
                            }
                            else
                            if (argumentType.IsAssignableFrom(typeof(byte[]))) {
                                using (var memoryStream = new MemoryStream()) {
                                    context.Request.InputStream.CopyTo(memoryStream);
                                    var inputData = memoryStream.ToArray();
                                    result = Signature.Invoke(serviceInstance, new[] { inputData });
                                }
                            }
                            else
                                throw new NotImplementedException("Type not supported");
                        }
                        else
                        if (context.Request.ContentType == "application/x-www-form-urlencoded") {
                            using (var reader = new StreamReader(context.Request.InputStream)) {
                                var inputString = reader.ReadToEnd();
                                result = Signature.Invoke(serviceInstance, GetParameterValues(Signature, contractSerializer, reader.ReadToEnd()));
                            }
                        }
                        else
                        if (context.Request.ContentType == "multipart/form-data") {
                            throw new NotImplementedException();
                        }
                        else { // JSON or whatever string serialization format is used is default
                            using (var reader = new StreamReader(context.Request.InputStream)) { // this assumes JSON or other text format
                                var inputString = reader.ReadToEnd();
                                // TODO: build custom type from signature if possible! (see: https://stackoverflow.com/questions/3862226/how-to-dynamically-create-a-class-in-c)
                                var inputData = contractSerializer.Deserialize(inputString);
                                var inputValues = Signature.GetParameters().Select(i => ((IDictionary<string, object>)inputData)[i.Name]).ToArray();
                                result = Signature.Invoke(serviceInstance, inputValues);
                            }
                        }
                        break;
                    default:
                        throw new NotImplementedException("Method not supported");

                }
            }
            catch (HttpException x) {
                context.Response.StatusCode = (int)x.HttpStatusCode;
                if (x.HttpStatusDescription != null) context.Response.StatusDescription = x.HttpStatusDescription;
                context.Response.SendText(x.HttpStatusDescription);
                context.Response.Close();
                return true;
            }
#if DEBUG
            catch (Exception x) {
#else
            catch (Exception) {
#endif
                context.Response.StatusCode = 400;
#if DEBUG
                context.Response.SendText(x.InnerException?.Message ?? x.Message);
#else
                context.Response.SendText(context.Response.StatusDescription, "text/plain");
#endif
                context.Response.Close();
                return true;
            }
            context.Response.Headers["Server"] = "WOOF";
            if (Signature.ReturnType == typeof(void)) {
                context.Response.StatusCode = 200;
                context.Response.Close();
                return true;
            }
            if (result is Stream) {
                context.Response.SendStream((Stream)result, Metadata.ReturnContentType);
            }
            else {
                var serializedResult = contractSerializer.Serialize(result, Signature.ReturnType);
                context.Response.SendText(serializedResult, contractSerializer.ContentType);
            }
            return true;
        }

        /// <summary>
        /// Regular expression matching the text in curly braces.
        /// </summary>
        private static readonly Regex RxCurlyBracesContent = new Regex(@"\\{([^}]+)}", RegexOptions.Compiled);

    }

}
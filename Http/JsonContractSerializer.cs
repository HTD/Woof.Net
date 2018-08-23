using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace Woof.Net.Http {

    /// <summary>
    /// <see cref="Newtonsoft.Json"/> <see cref="IContractSerializer"/> implementation.
    /// </summary>
    public class JsonContractSerializer : IContractSerializer {

        /// <summary>
        /// Gets the serialization settings.
        /// </summary>
        public JsonSerializerSettings Settings { get; }

        /// <summary>
        /// Creates the serializer object and initializes it with settings.
        /// </summary>
        /// <param name="settings">Optional custom settings for serialization.</param>
        public JsonContractSerializer(JsonSerializerSettings settings = null) => Settings = settings ?? new JsonSerializerSettings();

        /// <summary>
        /// Gets the content type for serialized data.
        /// </summary>
        public string ContentType => "application/json";

        /// <summary>
        /// Deserializes JSON to an object.
        /// </summary>
        /// <param name="data">Serialized data.</param>
        /// <returns>Expando object.</returns>
        public object Deserialize(string data) {
            var x = new ExpandoObject() as IDictionary<string, object>;
            if (RxJsonDateTime.IsMatch(data)) data = '"' + data + '"';
            if (
                data[0] != '"' &&
                data[0] != '{' &&
                data[0] != '[' &&
                (data[0] < '0' || data[0] > '9') &&
                !(data[0] == '-' && data.Length > 1 && data[1] >= '0' && data[1] <= '9') &&
                data != "true" && data != "false"
            ) return data;
            var obj = JsonConvert.DeserializeObject(data);
            if (obj is JObject jso) {
                foreach (var p in jso) {
                    switch (p.Value.Type) {
                        case JTokenType.Null: x[p.Key] = null; break;
                        case JTokenType.Boolean: x[p.Key] = (bool)p.Value; break;
                        case JTokenType.Integer: x[p.Key] = (long)p.Value; break;
                        case JTokenType.Float: x[p.Key] = (double)p.Value; break;
                        case JTokenType.Date: x[p.Key] = (DateTime)p.Value; break;
                        case JTokenType.TimeSpan: x[p.Key] = (TimeSpan)p.Value; break;
                        case JTokenType.String: x[p.Key] = (string)p.Value; break;
                        case JTokenType.Bytes: x[p.Key] = (byte[])p.Value; break;
                        default: x[p.Key] = p.Value; break;
                    }
                }
                return x;
            }
            else return obj;
        }

        /// <summary>
        /// Deserializes JSON to an object of specified type.
        /// </summary>
        /// <param name="data">Serialized data.</param>
        /// <param name="type">Target type.</param>
        /// <returns>Deserialized object of specified type.</returns>
        public object Deserialize(string data, Type type) {
            if (data == null) return null;
            if (type == typeof(string)) return data;
            if (RxJsonDateTime.IsMatch(data)) data = '"' + data + '"';
            return JsonConvert.DeserializeObject(data, type);
        }

        /// <summary>
        /// Deserializes JSON to the specified type.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="data">Serialized data.</param>
        /// <returns>Deserialized type.</returns>
        public T Deserialize<T>(string data) {
            if (typeof(T) == typeof(string)) return (T)(object)data;
            if (RxJsonDateTime.IsMatch(data)) data = '"' + data + '"';
            return JsonConvert.DeserializeObject<T>(data);
        }

        /// <summary>
        /// Serializes the data object to JSON.
        /// </summary>
        /// <param name="data">Data object.</param>
        /// <returns>JSON serialized data.</returns>
        public string Serialize(object data) => JsonConvert.SerializeObject(data, Settings);

        /// <summary>
        /// Serializes the specified type instance to JSON.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <param name="data">Data object.</param>
        /// <returns>JSON serialized data.</returns>
        public string Serialize<T>(T data) => JsonConvert.SerializeObject(data, typeof(T), Settings);

        /// <summary>
        /// Serializes data object of specified type to JSON.
        /// </summary>
        /// <param name="data">Data object.</param>
        /// <param name="type">Data type.</param>
        /// <returns>JSON serialized data.</returns>
        public string Serialize(object data, Type type) => JsonConvert.SerializeObject(data, type, Settings);

        private static readonly Regex RxJsonDateTime = new Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{3}Z$", RegexOptions.Compiled);

    }

}
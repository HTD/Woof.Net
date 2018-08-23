using System;
using System.Dynamic;

namespace Woof.Net.Http {

    /// <summary>
    /// Defines data serializer for contracts.
    /// </summary>
    public interface IContractSerializer {

        /// <summary>
        /// Gets the content type for serialized data.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Serializes data to string.
        /// </summary>
        /// <param name="data">Data object.</param>
        /// <returns>Serialized data.</returns>
        string Serialize(object data);

        /// <summary>
        /// Serializes data to string.
        /// </summary>
        /// <typeparam name="T">Data type to serialize.</typeparam>
        /// <param name="data">Data object.</param>
        /// <returns>Serialized data.</returns>
        string Serialize<T>(T data);

        /// <summary>
        /// Serializes typed data to string.
        /// </summary>
        /// <param name="data">Data type to serialize.</param>
        /// <param name="type">Source type.</param>
        /// <returns>Serialized data.</returns>
        string Serialize(object data, Type type);

        /// <summary>
        /// Deserializes data from string to generic type (check for <see cref="ExpandoObject"/>).
        /// </summary>
        /// <param name="data">Serialized data.</param>
        /// <returns>Data object.</returns>
        object Deserialize(string data);

        /// <summary>
        /// Deserialized data to specified type.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        object Deserialize(string data, Type type);

        /// <summary>
        /// Deserializes data from string.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="data">Serialized data.</param>
        /// <returns>Data object.</returns>
        T Deserialize<T>(string data);

    }

}
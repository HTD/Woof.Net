using System.IO;

namespace Woof.Net.Http {

    /// <summary>
    /// Allows to attach to normalize file system access.
    /// </summary>
    public interface IReadOnlyFileSystemAdapter {

        /// <summary>
        /// Checks if the file exists in implemented file system.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>True if the file exists.</returns>
        bool FileExists(string path);

        /// <summary>
        /// Checks if the directory exists in implemented file system.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Normalize relative paths to absolute if applicable.
        /// </summary>
        /// <param name="path">Absolute or relative path.</param>
        /// <returns>Absolute path.</returns>
        string NormalizePath(string path);

        /// <summary>
        /// Gets the file stream in non-blocking, read-only manner.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>Read only stream.</returns>
        Stream GetStream(string path);

    }

}
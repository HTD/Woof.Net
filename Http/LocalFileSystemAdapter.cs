using System.IO;

namespace Woof.Net.Http {

    /// <summary>
    /// File system adapter implementing local files and directories.
    /// </summary>
    public class LocalFileSystemAdapter : IReadOnlyFileSystemAdapter {

        /// <summary>
        /// Checks if the directory exists in the local file system.
        /// </summary>
        /// <param name="path">Path to the directory.</param>
        /// <returns>True if directory exists.</returns>
        public bool DirectoryExists(string path) => Directory.Exists(path);

        /// <summary>
        /// Checks if the file exists in the local file system.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>True if file exists.</returns>
        public bool FileExists(string path) => File.Exists(path);

        /// <summary>
        /// Normalize relative paths to absolute if applicable.
        /// </summary>
        /// <param name="path">Absolute or relative path.</param>
        /// <returns>Absolute path.</returns>
        public string NormalizePath(string path) => Path.GetFullPath(path);

        /// <summary>
        /// Gets the local file stream.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>File stream.</returns>
        public Stream GetStream(string path) => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

    }

}
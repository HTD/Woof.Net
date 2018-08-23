using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Woof.Net.Http {

    /// <summary>
    /// <see cref="IReadOnlyFileSystemAdapter"/> implementing assembly manifest resources virtual file system.
    /// </summary>
    public class ResourceStreamAdapter : IReadOnlyFileSystemAdapter {

        /// <summary>
        /// Creates new assembly manifest resources virtual file system.
        /// </summary>
        /// <param name="targetAssembly">Optional target assembly. Default is entry assembly.</param>
        public ResourceStreamAdapter(Assembly targetAssembly = null) {
            TargetAssembly = targetAssembly ?? Assembly.GetEntryAssembly();
            TargetAssemblyName = TargetAssembly.GetName().Name;
            Directory = TargetAssembly.GetManifestResourceNames().Select(i => i.Replace(TargetAssemblyName + '.', "")).ToArray();
        }

        /// <summary>
        /// Checks if specified virtual directory exists.
        /// Since assembly manifest resources don't have directories,
        /// it returns true if the beginning of the path matches one of the paths but the rest of the path doesn't.
        /// </summary>
        /// <param name="path">Path to the "directory".</param>
        /// <returns>True if "directory" exists.</returns>
        public bool DirectoryExists(string path) {
            var manifestResourcePath = GetManifestResourcePath(path);
            return Directory.Any(i => i.StartsWith(manifestResourcePath, StringComparison.OrdinalIgnoreCase)) &&
                !Directory.Any(i => i.Equals(manifestResourcePath, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks if specified file exists.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>True if file exists.</returns>
        public bool FileExists(string path) {
            var manifestResoucePath = GetManifestResourcePath(path);
            return Directory.Any(i => i.Equals(manifestResoucePath, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the manifest resource stream pointed with local-like path.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>Manifest resource stream.</returns>
        public Stream GetStream(string path) => TargetAssembly.GetManifestResourceStream(TargetAssemblyName + '.' + GetManifestResourcePath(path));

        /// <summary>
        /// Empty normalizer, since all resource paths are relative ones.
        /// </summary>
        /// <param name="path">Original path.</param>
        /// <returns>Same path.</returns>
        public string NormalizePath(string path) => path;

        /// <summary>
        /// Converts local-like paths to more manifest resource paths.
        /// </summary>
        /// <param name="path">Local-like path.</param>
        /// <returns>Manifest resource-like path.</returns>
        private string GetManifestResourcePath(string path) => path.Replace(Path.DirectorySeparatorChar, '.');

        /// <summary>
        /// Target assembly.
        /// </summary>
        private readonly Assembly TargetAssembly;

        /// <summary>
        /// Target assembly name.
        /// </summary>
        private readonly string TargetAssemblyName;

        /// <summary>
        /// An array of manifest resource names without the assembly name.
        /// </summary>
        private readonly string[] Directory;

    }

}
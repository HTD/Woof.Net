using System;
using System.IO;
using System.Linq;

namespace Woof.Net.Http {

    /// <summary>
    /// Defines a binding between path prefix and document root.
    /// Allows serving a simple HTML site.
    /// </summary>
    public class SiteBinding {

        /// <summary>
        /// Gets the path prefix within the server prefix.
        /// </summary>
        public string PathPrefix { get; set; }

        /// <summary>
        /// Gets or sets the file system adapter. Default the <see cref="LocalFileSystemAdapter"/> is created.
        /// </summary>
        public IReadOnlyFileSystemAdapter FileSystemAdapter { get; set; } = new LocalFileSystemAdapter();

        /// <summary>
        /// Gets the document root location for binding.
        /// </summary>
        public string DocumentRoot { get; set; }

        /// <summary>
        /// Gets the default document for the directory path.
        /// </summary>
        public string DocumentDefault { get; set; } = "index.html";

        /// <summary>
        /// Gets the default 404 document for the missing content fallback HTML.
        /// </summary>
        public string Document404 { get; set; } = "404.html";

        /// <summary>
        /// Gets a value indicating whether the directory listing of the site is enabled.
        /// </summary>
        public bool IsDirectoryListingAllowed { get; set; }

        /// <summary>
        /// Gets the local document location for specified relative path.
        /// </summary>
        /// <param name="relativeUrl">Relative document URL, without local prefix.</param>
        /// <returns>Absolute path to the local file or directory.</returns>
        private string GetLocalPath(string relativeUrl) {
            var sysDirectorySeparatorChar = Path.DirectorySeparatorChar;
            var urlDirectorySeparatorChar = '/';
            if (DocumentRoot == null || relativeUrl == null) return null;
            var rootNormalized = FileSystemAdapter.NormalizePath(DocumentRoot).TrimEnd(Path.DirectorySeparatorChar);
            if (relativeUrl == String.Empty || relativeUrl == urlDirectorySeparatorChar.ToString()) return rootNormalized;
            return
                rootNormalized +
                sysDirectorySeparatorChar +
                relativeUrl.Trim(urlDirectorySeparatorChar).Replace(urlDirectorySeparatorChar, sysDirectorySeparatorChar);
        }

        /// <summary>
        /// Processes incoming HTTP request from the server. Serves local documents directly. Returns true if the request was handled here.
        /// </summary>
        /// <param name="context">HTTP server context.</param>
        /// <returns>True if the request was handled, false otherwise.</returns>
        internal bool ProcessHttpRequest(ServerContext context) {
            if (DocumentRoot == null || context.RequestPath == null) return false;
            if (context.TryResolveLocalPrefix(PathPrefix, out var relativeUrl)) {
                var localPath = GetLocalPath(relativeUrl);
                var existingFile = FileSystemAdapter.FileExists(localPath);
                var existingDir = !existingFile && FileSystemAdapter.DirectoryExists(localPath);
                var missingSlash = existingDir && !relativeUrl.EndsWith("/");
                if (missingSlash) {
                    context.Response.Redirect(context.Request.Url.OriginalString + '/');
                    context.Response.Close();
                    return true;
                }
                context.Response.Headers["Server"] = "WOOF";
                bool respondWithLocalPathTarget() {
                    context.Response.SendStream(
                        FileSystemAdapter.GetStream(localPath),
                        MimeMapping.GetMimeType(localPath)
                    );
                    return true;
                }
                if (existingFile) return respondWithLocalPathTarget();
                if (existingDir) {
                    localPath = GetLocalPath(DocumentDefault);
                    if (FileSystemAdapter.FileExists(localPath)) return respondWithLocalPathTarget();
                    if (IsDirectoryListingAllowed) {
                        throw new NotImplementedException();
                    } else {
                        context.Response.StatusCode = 403;
                        context.Response.StatusDescription = "Directory listing not allowed";
                        context.Response.Close();
                    }
                    return true;
                }
                // 404 case:
                context.Response.StatusCode = 404;
                if (!context.Request.AcceptTypes.Any(i => i.Contains("html"))) {
                    context.Response.Close();
                    return true;
                }
                localPath = GetLocalPath(Document404);
                if (FileSystemAdapter.FileExists(localPath)) return respondWithLocalPathTarget();
                else context.Response.Close();
                return true;
            }
            return false;
        }

    }

}
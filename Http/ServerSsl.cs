using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Woof.Net.Http {

    /// <summary>
    /// <see cref="Server"/> SSL support.
    /// </summary>
    public partial class Server {

        /// <summary>
        /// Configures certificates for end points.
        /// </summary>
        /// <param name="thumbPrints">Thumb prints for each configured prefix.</param>
        public void SslConfigure(params string[] thumbPrints) {
            var appId = (string)
                Assembly.GetEntryAssembly()
                .GetCustomAttributesData()
                .FirstOrDefault(i => i.AttributeType.Name == "GuidAttribute")
                .ConstructorArguments[0]
                .Value;
            if (Prefixes.Length == thumbPrints.Length) {
                var isOk = true;
                for (int i = 0, n = Prefixes.Length; i < n; i++) {
                    if (String.IsNullOrEmpty(Prefixes[i])) continue;
                    var uri = new Uri(Prefixes[i]);
                    var hostname = uri.Host;
                    var port = uri.Port;
                    var hash = thumbPrints[i];
                    var existing = SslGetCertProperties(hostname, port);
                    if (existing.Any()) { // already bound
                        if (existing["ApplicationID"].Trim('{', '}') == appId && existing["CertificateHash"] == hash)
                            continue; // already bound, nothing else to do.
                        else isOk = isOk && SslDeleteCert(hostname, port);
                    }
                    if (!String.IsNullOrEmpty(hash)) isOk = isOk && SslAddCert(hostname, port, appId, hash);
                }
                if (!isOk) throw new InvalidOperationException("Could not configure SSL binding for one of the thumb prints");
            }
            else throw new InvalidOperationException("Thumb prints count doesn't match prefixes count");
        }

        /// <summary>
        /// Enumerates all available certificate stores in specifed location.
        /// </summary>
        /// <param name="storeLocation">Store location.</param>
        /// <returns>Store name array.</returns>
        private string[] SslGetStores(StoreLocation storeLocation) {
            var commandLine = $"-Command \"dir cert:\\{storeLocation}\"";
            var startInfo = new ProcessStartInfo("powershell", commandLine) { UseShellExecute = false, RedirectStandardOutput = true };
            var process = Process.Start(startInfo);
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            return output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Replace("Name : ", "")).ToArray();
        }

        /// <summary>
        /// Finds SSL Certificate store for given thumb print.
        /// </summary>
        /// <param name="thumbPrint">Certificate hash.</param>
        /// <param name="storeLocation">Store location.</param>
        /// <returns>Store name or null if not found.</returns>
        private string SslFindStore(string thumbPrint, StoreLocation storeLocation) {
            var storeNames = SslGetStores(storeLocation);
            foreach (var storeName in storeNames) {
                using (var store = new X509Store(storeName, StoreLocation.LocalMachine)) {
                    try {
                        store.Open(OpenFlags.ReadOnly);
                        var matches = store.Certificates.Find(X509FindType.FindByThumbprint, thumbPrint, false);
                        if (matches.Count > 0) {
                            
                            store.Close();
                            return storeName;
                        }
                    }
                    catch (Exception) { } // if can't open, just give up.
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the X509 certificate properties for specified host name and port.
        /// </summary>
        /// <param name="hostname">Host name.</param>
        /// <param name="port">Port number.</param>
        /// <returns>Properties dictionary.</returns>
        private Dictionary<string, string> SslGetCertProperties(string hostname, int port) {
            var commandLine = $"http show sslcert hostnameport={hostname}:{port}";
            var startInfo = new ProcessStartInfo("netsh", commandLine) { UseShellExecute = false, RedirectStandardOutput = true };
            var process = Process.Start(startInfo);
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            var lines = output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            return lines
                .Select(l => l.Split(new string[] { " : " }, StringSplitOptions.RemoveEmptyEntries))
                .Where(s => s.Length == 2)
                .Select(s => new KeyValuePair<string, string>(s[0].Replace(" ", ""), s[1].Trim()))
                .ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>
        /// Adds SSL certificate for the specified host name and port.
        /// </summary>
        /// <param name="hostname">Host name.</param>
        /// <param name="port">Port number.</param>
        /// <param name="appId">Application GUID as string, no curly braces.</param>
        /// <param name="certHash">Certificate thumprint.</param>
        /// <returns>True if added successfully, false if error occured.</returns>
        private bool SslAddCert(string hostname, int port, string appId, string certHash) {
            var certStoreName = SslFindStore(certHash, StoreLocation.LocalMachine);
            if (certStoreName == null) throw new InvalidOperationException("X509 certificate not found in LocalMachine stores");
            var commandLine =
                @"http add sslcert" +
                @" hostnameport=" + hostname + ':' + port.ToString() +
                @" certhash=" + certHash +
                @" appid={" + appId + @"}" +
                @" certstorename=" + certStoreName;
            var startInfo = new ProcessStartInfo("netsh", commandLine) { UseShellExecute = false, RedirectStandardOutput = true };
            var process = Process.Start(startInfo);
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        /// <summary>
        /// Deletes SSL certificate for the specified host name and port.
        /// </summary>
        /// <param name="hostname">Host name.</param>
        /// <param name="port">Port number.</param>
        /// <returns>True if deleted successfully, false if error occured.</returns>
        private bool SslDeleteCert(string hostname, int port) {
            var commandLine = $"http delete sslcert hostnameport={hostname}:{port}";
            var startInfo = new ProcessStartInfo("netsh", commandLine) { UseShellExecute = false, RedirectStandardOutput = true };
            var process = Process.Start(startInfo);
            process.WaitForExit();
            return process.ExitCode == 0;
        }

    }

}
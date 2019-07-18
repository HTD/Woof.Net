using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Woof.Net.Http {

    /// <summary>
    /// <see cref="Server"/> SSL support.
    /// </summary>
    public partial class Server {

        /// <summary>
        /// Configures certificates for end points. Requires administrator privileges.
        /// </summary>
        private void SslAutoConfigure() {
            var appId = (string)
                Assembly.GetEntryAssembly()
                .GetCustomAttributesData()
                .FirstOrDefault(i => i.AttributeType.Name == "GuidAttribute")?
                .ConstructorArguments[0]
                .Value;
            if (appId == null) return;
            for (int i = 0, n = Prefixes.Length; i < n; i++) {
                if (String.IsNullOrEmpty(Prefixes[i])) continue;
                var uri = new Uri(Prefixes[i]);
                if (uri.Scheme != "https") continue;
                if (appId == null) throw new InvalidOperationException("Application GUID must be set to use SSL endpoint");
                var hostname = uri.Host;
                var port = uri.Port;
                var existing = SslGetCertProperties(hostname, port);
                if (SslFindCertificate(hostname, StoreLocation.LocalMachine, out var storeName, out var cert)) {
                    if (existing.Any()) { // already bound
                        if (existing["ApplicationID"].Trim('{', '}') == appId && existing["CertificateHash"].Equals(cert.Thumbprint, StringComparison.OrdinalIgnoreCase))
                            continue; // already bound, nothing else to do.
                        else {
                            if (!SslDeleteCert(hostname, port))
                                throw new InvalidOperationException($"Can't delete certificate binding for {hostname}:{port}");
                            Thread.Sleep(100); // just in case it would be performed asynchronously internally by OS.
                        }
                    }
                    if (!SslAddCert(hostname, port, appId, cert.Thumbprint, storeName))
                        throw new InvalidOperationException($"Can't add certificate binding for {hostname}:{port}");
                }
                else throw new InvalidOperationException($"VALID certificate for {hostname} not found in {StoreLocation.LocalMachine}'s stores");
            }
        }

        /// <summary>
        /// Enumerates all available certificate stores in specifed location.
        /// </summary>
        /// <param name="storeLocation">Store location.</param>
        /// <returns>Store name array.</returns>
        private string[] SslGetStores(StoreLocation storeLocation = StoreLocation.LocalMachine) {
            var commandLine = $"-Command \"dir cert:\\{storeLocation}\"";
            var startInfo = new ProcessStartInfo("powershell", commandLine) { UseShellExecute = false, RedirectStandardOutput = true };
            var process = Process.Start(startInfo);
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            return output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Replace("Name : ", "")).ToArray();
        }

        /// <summary>
        /// Finds the matching certificate for a host.
        /// </summary>
        /// <param name="host">Host name (domain).</param>
        /// <param name="storeLocation">Store location.</param>
        /// <param name="storeName">Store name where the certificate was found.</param>
        /// <param name="certificate">X509 Certificate found.</param>
        /// <returns>True if valid certificate found.</returns>
        public bool SslFindCertificate(string host, StoreLocation storeLocation, out string storeName, out X509Certificate2 certificate) {
            var storeNames = SslGetStores(storeLocation);
            for (int i = 0, n = storeNames.Length; i < n; i++) {
                storeName = storeNames[i];
                using (var store = new X509Store(storeName, StoreLocation.LocalMachine)) {
                    try {
                        store.Open(OpenFlags.ReadOnly);
                        //var certs =
                        //    store
                        //    .Certificates
                        //    .Find(X509FindType.FindByTimeValid, DateTime.Now, true)
                        //    .OfType<X509Certificate2>();
                        //;
                        certificate =
                            store
                            .Certificates
                            .Find(X509FindType.FindByTimeValid, DateTime.Now, true)
                            .OfType<X509Certificate2>()
                            .FirstOrDefault(c => {
                                var cn = c.GetNameInfo(X509NameType.DnsName, forIssuer: false);
                                if (String.IsNullOrEmpty(cn)) return false;
                                return host.EndsWith(cn.Replace("*.", ""));
                            });
                        if (certificate != null) return true;
                    }
                    catch (Exception) { } // if can't open, just give up.
                }
            }
            storeName = null;
            certificate = null;
            return false;
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
                .Skip(2).TakeWhile(i => !i.Contains("Extended Properties:"))
                .Select(l => l.Split(new string[] { " : " }, StringSplitOptions.RemoveEmptyEntries))
                .Where(s => s.Length == 2)
                .Select(s => new KeyValuePair<string, string>(s[0].Replace(" ", ""), s[1].Trim()))
                .Where(p => p.Key != "PropertyId" && !String.IsNullOrEmpty(p.Value))
                .ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>
        /// Adds SSL certificate for the specified host name and port.
        /// </summary>
        /// <param name="hostname">Host name.</param>
        /// <param name="port">Port number.</param>
        /// <param name="appId">Application GUID as string, no curly braces.</param>
        /// <param name="certHash">Certificate thumprint.</param>
        /// <param name="certStoreName">Certificate store name.</param>
        /// <returns>True if added successfully, false if error occured.</returns>
        private bool SslAddCert(string hostname, int port, string appId, string certHash, string certStoreName) {
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
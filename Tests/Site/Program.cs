using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Woof.Net.Http;

namespace Woof.Net.Tests {

    static class Program {

        private const string Prefix = "https://codedog.test.net/";

        /// <summary>
        /// Tests the site and the REST service mode.
        /// </summary>
        /// <param name="args">Ignored.</param>
        public static void Main(params string[] args) {
            Console.WriteLine($"Starting WOOF HTTP server on {Prefix}...");
            using (var server = new Server(Prefix)) {
                server.SiteBindings.Add(new SiteBinding { FileSystemAdapter = new ResourceStreamAdapter(), DocumentRoot = "TestSite" });
                server.ServiceBindings.Add(new WaitTestService());
                server.Start();

                Task.Run(() => SendWaitRequests(server.MaxConcurrentRequests - 1));
                Console.WriteLine("Press Enter to send release signal...");
                Console.ReadLine();
                Task.Run(() => SendRelease());


                Console.WriteLine("Press Enter to shut down server...");
                Console.ReadLine();

            }
            Console.WriteLine("Server shut down successfully.");
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        /// <summary>
        /// Sends n parallel wait requests.
        /// </summary>
        /// <param name="n">Number of requests to send.</param>
        static void SendWaitRequests(int n) => Parallel.For(0, n, i => {
            Console.WriteLine($"Sending request id={i}...");
            var req = WebRequest.Create($"{Prefix}waitEvent/" + i);
            req.Timeout = 3600000;
            using (var response = req.GetResponse()) Console.WriteLine($"Request id={i} completed.");
        });

        /// <summary>
        /// Sends release signal.
        /// </summary>
        static void SendRelease() {
            Console.WriteLine("Sending release request.");
            var req = WebRequest.Create($"{Prefix}release");
            using (var response = req.GetResponse()) Console.WriteLine($"Release request done.");
        }

        /// <summary>
        /// A test service.
        /// </summary>
        [ServiceContract]
        class WaitTestService {

            /// <summary>
            /// A semaphore used to synchronize multiple requests.
            /// </summary>
            MultiThreadSemaphore S = new MultiThreadSemaphore();

            /// <summary>
            /// Waits for a request.
            /// </summary>
            /// <param name="requestId">Unique request identifier.</param>
            /// <returns>OK.</returns>
            [OperationContract("get", "waitEvent/{requestId}")]
            public string WaitEvent(long requestId) {
                S.WaitEvent();
                return "OK";
            }

            /// <summary>
            /// Releases all waiting threads.
            /// </summary>
            /// <returns>OK.</returns>
            [OperationContract("get", "release")]
            public string Release() {
                S.ReleaseAll();
                return "OK";
            }

            /// <summary>
            /// Test with example: http://localhost/synthetic/true/5/0.1234/2018-08-21T18:02:16.447Z/hello
            /// </summary>
            /// <param name="b"></param>
            /// <param name="i"></param>
            /// <param name="d"></param>
            /// <param name="dt"></param>
            /// <param name="s"></param>
            /// <returns></returns>
            [OperationContract("get", "synthetic/{b}/{i}/{d}/{dt}/{s}")]
            public object Synthetic(bool b, int i, decimal d, DateTime dt, string s) => new {
                BoolValue = b,
                IntValue = i,
                DecimalValue = d,
                DateTimeValue = dt,
                StringValue = s
            };

            /// <summary>
            /// Tests basic GET binding.
            /// </summary>
            /// <param name="a">One integer.</param>
            /// <param name="b">Other integer.</param>
            /// <returns>The sum of integers.</returns>
            [OperationContract]
            public int Add(int a, int b) => a + b;

            /// <summary>
            /// Test of pattern GET binding and internal error handling.
            /// </summary>
            /// <param name="a">One number.</param>
            /// <param name="b">Other number, try zero to throw <see cref="DivideByZeroException"/>.</param>
            /// <returns>Division result.</returns>
            [OperationContract("GET", "div:{a}/{b}")]
            public decimal Div(decimal a, decimal b) => a / b;

            /// <summary>
            /// Reads JSON from POST request.
            /// </summary>
            /// <param name="s">A string.</param>
            /// <param name="i">A long integer number.</param>
            /// <param name="dt">A date/time in JSON format.</param>
            /// <returns></returns>
            [OperationContract("post", "readJson")]
            public string ReadJson(string s, long i, DateTime dt) => $"Got {s}, {i} and {dt}.";

        }

    }

}
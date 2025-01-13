using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Reflection;
using Grpc.Net.Client.Web;


namespace ClientFramework
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Framework Client is starting");

            var t = Task.Run(async () =>
            {
                GRPCClient client = new GRPCClient("localhost", "https", 7274);
                await Task.Delay(10000);

                client.Communicate();
            });
                       

            while (true)
            {
                Task.WaitAll(t);
            }
        }
    }

    //https://learn.microsoft.com/it-it/aspnet/core/grpc/supported-platforms?view=aspnetcore-9.0

    public class GRPCClient
    {
        public Uri Uri { get; set; } = null;
        private GrpcChannel _channel { get; set; } = null;
        public GRPCClient(string host, string scheme, int port)
        {
            var loggerFactory = LoggerFactory.Create(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Debug);
            });

            GrpcChannelOptions opt = new GrpcChannelOptions()
            {
                LoggerFactory = loggerFactory,

                HttpHandler = new WinHttpHandler()
                {
                    ServerCertificateValidationCallback = Validate,
                    SslProtocols = System.Security.Authentication.SslProtocols.Tls12
                }
            };

            Uri = new UriBuilder(scheme, host, port).Uri;
            _channel = GrpcChannel.ForAddress(Uri.AbsoluteUri, opt);
        }

        public bool Validate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public async void Communicate()
        {
            int i = 0;
            try
            {
                Console.WriteLine($"Start Communication");
                var client = new GreeterTest.Greeter.GreeterClient(_channel);

                while (true)
                {
                    await Task.Delay(500);
                    var response = await client.SayHelloAsync(new GreeterTest.HelloRequest { Name = $"FRAMEWORK 4.7.2, ID[{i++}]" });
                    Console.WriteLine($"R: {response.Message}");
                }
            }
            catch
            {
                //Console.WriteLine($"Error {ex.Message}");
            }
        }
    }
}

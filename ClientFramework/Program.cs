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
                GRPCClient client = new GRPCClient("localhost", "http", 5062);
                await Task.Delay(10000);

                client.Communicatee();


            });
                       

            while (true)
            {
                Task.WaitAll(t);
            }
        }
    }

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
                HttpVersion = new Version("1.1"),

                HttpHandler = new HttpClientHandler()
                {

                }
            };

            Uri = new UriBuilder(scheme, host, port).Uri;
            _channel = GrpcChannel.ForAddress(Uri.AbsoluteUri, opt);
        }

        public bool Validate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
        public async void Communicatee()
        {
            try
            {
                Console.WriteLine($"Start Communication");
                var client = new GreeterTest.Greeter.GreeterClient(_channel);
                var response = await client.SayHelloAsync(new GreeterTest.HelloRequest { Name = "FRAMEWORK 4.7.2" });
                Console.WriteLine($"R: {response.Message}");

            }
            catch(Exception ex) 
            {
                //Console.WriteLine($"Error {ex.Message}");
            }
        }
    }
}

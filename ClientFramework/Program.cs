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
using Grpc.Core;
using System.Threading;


namespace ClientFramework
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Framework Client is starting");

            var t = Task.Run(async () =>
            {
                GRPCFrameworkClient client = new GRPCFrameworkClient("localhost", "https", 7274);
                await Task.Delay(10000);

                client.Communicate();

                client.CommunicateStream();
            });

            var t1 = Task.Run(async () =>
            {
                //GRPCFrameworkClient_gRPC_Web client = new GRPCFrameworkClient_gRPC_Web("localhost", "https", 7274);
                //await Task.Delay(10000);

                //client.Communicate();

                //client.CommunicateStream();
            });

            while (true)
            {
                Task.WaitAll(t, t1);
            }
        }
    }

    public class GRPCFrameworkClient_gRPC_Web : GRPCFrameworkClient
    {
        public GRPCFrameworkClient_gRPC_Web(string host, string scheme, int port) : base(host, scheme, port)
        {

        }

        protected override string ClientName { get; set; } = "Framework GRPCWEB 4.7.2";
        public override void CreateChannel(ILoggerFactory loggerFactory)
        {
            GrpcChannelOptions opt = new GrpcChannelOptions()
            {
                LoggerFactory = loggerFactory,

                HttpHandler = new GrpcWebHandler(new WinHttpHandler()
                {
                    ServerCertificateValidationCallback = Validate,
                    SslProtocols = System.Security.Authentication.SslProtocols.Tls12
                })
            };

            _channel = GrpcChannel.ForAddress(Uri.AbsoluteUri, opt);
        }
    }

    //https://learn.microsoft.com/it-it/aspnet/core/grpc/supported-platforms?view=aspnetcore-9.0

    public class GRPCFrameworkClient
    {
        public Uri Uri { get; set; } = null;
        protected GrpcChannel _channel { get; set; } = null;
        public GRPCFrameworkClient(string host, string scheme, int port)
        {
            Uri = new UriBuilder(scheme, host, port).Uri;
            var loggerFactory = LoggerFactory.Create(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Debug);
            });

            CreateChannel(loggerFactory);
        }

        public virtual void CreateChannel(ILoggerFactory loggerFactory)
        {
            GrpcChannelOptions opt = new GrpcChannelOptions()
            {
                LoggerFactory = loggerFactory,

                HttpHandler = new WinHttpHandler()
                {
                    ServerCertificateValidationCallback = Validate,
                    SslProtocols = System.Security.Authentication.SslProtocols.Tls12
                }
            };

            _channel = GrpcChannel.ForAddress(Uri.AbsoluteUri, opt);
        }

        public bool Validate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
        protected virtual string ClientName { get; set; } = "FRAMEWORK 4.7.2";
        CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();
        public async void Communicate()
        {
            int i = 0;
            try
            {
                Console.WriteLine($"Start Communication");
                var client = new GreeterTest.Greeter.GreeterClient(_channel);


                var response = await client.SayHelloAsync(new GreeterTest.HelloRequest { Name = $"{ClientName}" });
                Console.WriteLine($"R: {response.Message}");

            }
            catch
            {
                //Console.WriteLine($"Error {ex.Message}");
            }
        }

        public async void CommunicateStream()
        {
            try
            {
                Console.WriteLine($"Start Communication");
                var client = new GreeterTest.Greeter.GreeterClient(_channel);

                await Task.Delay(500);
                var streaming = client.SayStreamingHello(new GreeterTest.HelloRequest() { Name = ClientName }, cancellationToken: TokenSource.Token);

                while (await streaming.ResponseStream.MoveNext())
                {
                    var responseMsg = streaming.ResponseStream.Current;
                    Console.WriteLine($"RESPONSE: {responseMsg.Message}");
                }
            }
            catch
            {
                //Console.WriteLine($"Error {ex.Message}");
            }
        }
    }
}

using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.Extensions.Logging;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ClientCore
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Core Client is starting");

            var t = Task.Run(async () =>
            {
                GRPCCoreClient client = new GRPCCoreClient("localhost", "https", 7274);
                await Task.Delay(10000);

                client.Communicate();

                client.CommunicateStream();
            });


            while (true)
            {
                Task.WaitAll(t);
            }
        }
    }

    public class GRPCCoreClient
    {
        public Uri? Uri { get; set; } = null;
        private GrpcChannel? _channel { get; set; } = null;
        public GRPCCoreClient(string host, string scheme, int port)
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
                //HttpVersion = new Version("1.0"),

                //https://github.com/grpc/grpc-dotnet/issues/1961
                HttpHandler = new SocketsHttpHandler()
                {
                    ConnectTimeout = TimeSpan.FromSeconds(60),
                    AllowAutoRedirect = true,

                    SslOptions = new SslClientAuthenticationOptions()
                    {
                        RemoteCertificateValidationCallback = Validate
                    }
                }
            };


            _channel = GrpcChannel.ForAddress(Uri.AbsoluteUri, opt);
        }

        public bool Validate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors errors)
        {
            return true;
        }

        CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();

        string ClientName { get; set; } = "CORE .NET";
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

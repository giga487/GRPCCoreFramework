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

    public class GRPCClient
    {
        public Uri? Uri { get; set; } = null;
        private GrpcChannel? _channel { get; set; } = null;
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

            Uri = new UriBuilder(scheme, host, port).Uri;
            _channel = GrpcChannel.ForAddress(Uri.AbsoluteUri, opt);
        }

        public bool Validate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors errors)
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
                    var response = await client.SayHelloAsync(new GreeterTest.HelloRequest { Name = $"CORE .NET, ID[{i++}]" });
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

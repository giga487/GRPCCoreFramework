using GreeterTest;
using Grpc.Core;
using SERVERCore;
using static SERVERCore.Services.GreeterService;

namespace SERVERCore.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Received: {request.Name}");
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public class Client<T>
        {
            public string Message { get; set; } = string.Empty;
            public int Iteraction { get; set; } = 0;
            public IServerStreamWriter<T>? ResponseStream { get; set; } = default(IServerStreamWriter<T>);
        }
        public Client<HelloReply>? SayHelloStreaming { get; set; } = null;
        public async override Task SayStreamingHello(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            try
            {
                timer = new System.Timers.Timer(TimeSpan.FromSeconds(5));
                timer.Elapsed += Timer_Elapsed;
                timer.Start();

                SayHelloStreaming = new Client<HelloReply>()
                {
                    ResponseStream = responseStream,
                    Message = request.Name
                };

                await Task.Delay(Timeout.Infinite, context.CancellationToken);
                timer.Stop();

                timer.Elapsed -= Timer_Elapsed;
            }
            catch
            {

            }
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (SayHelloStreaming is not null)
            {
                string message = $"{SayHelloStreaming.Message}: {SayHelloStreaming.Iteraction++}";
                SayHelloStreaming?.ResponseStream?.WriteAsync(new HelloReply() 
                { 
                    Message =  message 
                });
            }

        }

        public System.Timers.Timer timer { get; set; }
    }
}

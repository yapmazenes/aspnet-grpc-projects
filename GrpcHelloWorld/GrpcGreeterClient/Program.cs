using Grpc.Net.Client;
using System;
using System.Threading.Tasks;

namespace GrpcGreeterClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Listener started");

            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);

            var responseMessage = await client.SayHelloAsync(new HelloRequest
            {
                Name = "Enes Yapmaz"
            });

            Console.WriteLine("Greetings: " + responseMessage.Message);
            Console.ReadKey();
        }
    }
}

using Grpc.Net.Client;
using GrpcHelloWorldClient.Protos;
using System;
using System.Threading.Tasks;

namespace GrpcHelloWorldClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Listener started");

            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new HelloService.HelloServiceClient(channel);

            var responseMessage = await client.SayHelloAsync(new HelloRequest
            {
                Name = "Enes Yapmaz"
            });

            Console.WriteLine("Greetings: " + responseMessage.Message);
            Console.ReadKey();
        }
    }
}

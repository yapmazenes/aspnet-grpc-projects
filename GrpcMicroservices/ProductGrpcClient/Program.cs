using Grpc.Net.Client;
using ProductGrpc.Protos;
using System;
using System.Threading.Tasks;

namespace ProductGrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            await Task.Delay(2000);

            var client = new ProductProtoService.ProductProtoServiceClient(channel);

            //GetProductAsync
            Console.WriteLine("GetProductAsync started...");

            var response = await client.GetProductAsync(new GetProductRequest
            {
                ProductId = 1
            });

            Console.WriteLine("GetProductAsync Response: " + response.ToString());
            Console.ReadLine();
        }
    }
}

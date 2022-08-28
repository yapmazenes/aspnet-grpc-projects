using Grpc.Core;
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

            await GetProductAsync(client);
            await GetAllProducts(client);

            Console.ReadLine();
        }

        private static async Task GetAllProducts(ProductProtoService.ProductProtoServiceClient client)
        {
            //Console.WriteLine("GetAllProducts started...");

            //using (var clientAllProducts = client.GetAllProducts(new GetAllProductsRequest()))
            //{
            //    while (await clientAllProducts.ResponseStream.MoveNext(new System.Threading.CancellationToken()))
            //    {
            //        var currentProduct = clientAllProducts.ResponseStream.Current;

            //        Console.WriteLine(currentProduct);
            //    }
            //}

            //GetAllProducts with C# 9
            Console.WriteLine("GetAllProducts started...");
            var clientAllProducts = client.GetAllProducts(new GetAllProductsRequest());

            await foreach (var responseData in clientAllProducts.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine(responseData);
            }
        }

        private static async Task GetProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("GetProductAsync started...");

            var response = await client.GetProductAsync(new GetProductRequest
            {
                ProductId = 1
            });

            Console.WriteLine("GetProductAsync Response: " + response.ToString());
        }
    }
}

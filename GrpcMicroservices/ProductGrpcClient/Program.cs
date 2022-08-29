using Google.Protobuf.WellKnownTypes;
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
            await Task.Delay(2000);
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");

            var client = new ProductProtoService.ProductProtoServiceClient(channel);

            await GetProductAsync(client);
            await GetAllProducts(client);

            await AddProductAsync(client);

            await UpdateProductAsync(client);
            await DeleteProductAsync(client);

            await InsertBulkProductAsync(client);

            Console.ReadLine();
        }

        private static async Task InsertBulkProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("InsertBulkProductAsync started...");

            using var clientBulk = client.InsertBulkProduct();

            for (int i = 0; i < 3; i++)
            {
                var productModel = new ProductModel
                {
                    Name = $"Product-{i}",
                    Description = "Bulk inserted Product",
                    Price = 399,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                };

                await clientBulk.RequestStream.WriteAsync(productModel);
            }

            await clientBulk.RequestStream.CompleteAsync();

            var responseBulk = await clientBulk;

            Console.WriteLine($"Status: {responseBulk.Success}. Inserted Count: {responseBulk.InsertCount}");
        }

        private static async Task UpdateProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("UpdateProductAsync started...");

            var updateProductResponse = await client.UpdateProductAsync(new UpdateProductRequest
            {
                Product = new ProductModel
                {
                    ProductId = 1,
                    Name = "New Red Phone Mi10T",
                    Price = 699,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            });

            Console.WriteLine($"UpdateProductAsync Response: {updateProductResponse}");
        }

        private static async Task AddProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("AddProductAsync started...");
            var addProductResponse = await client.AddProductAsync(new AddProductRequest
            {
                Product = new ProductModel
                {
                    Name = "Red",
                    Description = "New Red Phone Mi10T",
                    Price = 699,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            });

            Console.WriteLine($"AddProduct Response: {addProductResponse}");
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

        private static async Task DeleteProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("DeleteProductAsync started...");

            var deleteProductResponse = await client.DeleteProductAsync(new DeleteProductRequest
            {
                ProductId = 3
            });

            Console.WriteLine($"DeleteProductAsync Response: {deleteProductResponse.Success}");
        }
    }
}

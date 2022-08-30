using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;
using ShoppingCartGrpc.Protos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShoppingCartWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(2000);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                //1 Create SC if not exist
                //2 Retrieve products from product grpc with server stream
                //3 Add SC items into SC with client stream

                using var scChannel = GrpcChannel.ForAddress(_configuration.GetValue<string>("WorkerService:ShoppingCartServerUrl"));
                var scClient = new ShoppingCartProtoService.ShoppingCartProtoServiceClient(scChannel);

                //1 Create SC if not exist
                var scModel = await GetOrCreateShoppingCartAsync(scClient);

                //open SC client stream
                using var scClientStream = scClient.AddItemIntoShoppingCart();

                //2 Retrieve products from product grpc with server stream
                using var productChannel = GrpcChannel.ForAddress(_configuration.GetValue<string>("WorkerService:ProductServerUrl"));
                var productClient = new ProductProtoService.ProductProtoServiceClient(productChannel);

                _logger.LogInformation("GetAllProducts started...");

                using var clientData = productClient.GetAllProducts(new GetAllProductsRequest());

                await foreach (var responseData in clientData.ResponseStream.ReadAllAsync())
                {
                    _logger.LogInformation("GetAllProducts Stream Response: {responseData}", responseData);

                    //3 Add SC items into SC with client stream
                    var addNewScItem = new AddItemIntoShoppingCartRequest
                    {
                        Username = "enes",
                        DiscountCode = "CODE_100",
                        NewCartItem = new ShoppingCartItemModel
                        {
                            ProductId = responseData.ProductId,
                            ProductName = responseData.Name,
                            Price = responseData.Price,
                            Color = "Black",
                            Quantity = 1
                        }
                    };

                    await scClientStream.RequestStream.WriteAsync(addNewScItem);

                    _logger.LogInformation("Shopping Cart client stream added new Item: {addNewItemSc}", addNewScItem);
                }

                await scClientStream.RequestStream.CompleteAsync();

                var addItemIntoSCResponse = scClientStream;

                _logger.LogInformation("AddItemIntoShoppingCart client Stream Response: {addItemIntoShoppingCartResponse}", addItemIntoSCResponse);

                await Task.Delay(_configuration.GetValue<int>("WorkerService:TaskInterval"), stoppingToken);
            }
        }

        private async Task<ShoppingCartModel> GetOrCreateShoppingCartAsync(
            ShoppingCartProtoService.ShoppingCartProtoServiceClient scClient)
        {
            //try to get SC 
            //create SC

            ShoppingCartModel shoppingCartModel;

            try
            {
                _logger.LogInformation("GetShoppingCartAsync started...");

                shoppingCartModel = await scClient.GetShoppingCartAsync(new GetShoppingCartRequest
                {
                    Username = _configuration.GetValue<string>("WorkerService:UserName")
                });

                _logger.LogInformation("GetShoppingCartAsync Response: {shoppingCartModel}", shoppingCartModel);
            }
            catch (RpcException e)
            {
                if (e.StatusCode == StatusCode.NotFound)
                {
                    _logger.LogInformation("CreateShoppingCartAsync started...");

                    shoppingCartModel = await scClient.CreateShoppingCartAsync(new ShoppingCartModel
                    {
                        Username = _configuration.GetValue<string>("WorkerService:UserName")
                    });

                    _logger.LogInformation("CreateShoppingCartAsync Response: {shoppingCartModel}", shoppingCartModel);
                }
                else
                {
                    throw e;
                }
            }

            return shoppingCartModel;

        }
    }
}

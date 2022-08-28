using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductGrpc.Data;
using ProductGrpc.Mapper;
using ProductGrpc.Models;
using ProductGrpc.Protos;
using System;
using System.Threading.Tasks;

namespace ProductGrpc.Services
{
    public class ProductService : ProductProtoService.ProductProtoServiceBase
    {
        private readonly ProductsContext _productsContext;
        private readonly ILogger<ProductService> _logger;
        private readonly IMapper _mapper;

        public ProductService(ProductsContext productsContext, ILogger<ProductService> logger, IMapper mapper)
        {
            _productsContext = productsContext ?? throw new ArgumentNullException(nameof(productsContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public override Task<Empty> Test(Empty request, ServerCallContext context)
        {
            return base.Test(request, context);
        }

        public override async Task<ProductModel> GetProduct(GetProductRequest request,
            ServerCallContext context)
        {
            var product = await _productsContext.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                                        $"Product with Id={request.ProductId} is not found"));
            }

            var productModel = _mapper.Map<ProductModel>(product);

            productModel.Status = Protos.ProductStatus.Instock;

            return productModel;
        }

        public override async Task GetAllProducts(GetAllProductsRequest request,
            IServerStreamWriter<ProductModel> responseStream, ServerCallContext context)
        {
            var productList = await _productsContext.Products.ToListAsync();

            foreach (var product in productList)
            {
                var productModel = _mapper.Map<ProductModel>(product);
                productModel.Status = Protos.ProductStatus.Instock;

                await responseStream.WriteAsync(productModel);
            }
        }

        public override async Task<ProductModel> AddProduct(AddProductRequest request, ServerCallContext context)
        {
            var product = _mapper.Map<Product>(request.Product);

            product.Status = Models.ProductStatus.INSTOCK;

            _productsContext.Add(product);

            await _productsContext.SaveChangesAsync();

            request.Product.ProductId = product.ProductId;

            return request.Product;
        }

        public override async Task<ProductModel> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
        {
            bool isExist = await _productsContext.Products.AnyAsync(p => p.ProductId == request.Product.ProductId);
            if (!isExist)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                                        $"Product with Id={request.Product.ProductId} is not found"));
            }

            var product = _mapper.Map<Product>(request.Product);

            _productsContext.Entry(product).State = EntityState.Modified;

            try
            {
                await _productsContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return request.Product;
        }

        public override async Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
        {
            var product = await _productsContext.Products.FindAsync(request.ProductId);

            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                                        $"Product with Id={request.ProductId} is not found"));
            }

            _productsContext.Products.Remove(product);

            var deletedCount = await _productsContext.SaveChangesAsync();

            var response = new DeleteProductResponse
            {
                Success = deletedCount > 0
            };

            return response;
        }

    }
}

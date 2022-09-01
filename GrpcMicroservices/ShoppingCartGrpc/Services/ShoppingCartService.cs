using AutoMapper;
using DiscountGrpc.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShoppingCartGrpc.Data;
using ShoppingCartGrpc.Models;
using ShoppingCartGrpc.Protos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCartGrpc.Services
{
    [Authorize]
    public class ShoppingCartService : ShoppingCartProtoService.ShoppingCartProtoServiceBase
    {
        private readonly ShoppingCartContext _shoppingCartContext;
        private readonly DiscountService _discountService;

        private readonly ILogger<ShoppingCartService> _logger;
        private readonly IMapper _mapper;

        public ShoppingCartService(ShoppingCartContext shoppingCartContext, ILogger<ShoppingCartService> logger, IMapper mapper, DiscountService discountService)
        {
            _shoppingCartContext = shoppingCartContext ?? throw new ArgumentNullException(nameof(shoppingCartContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
        }

        public override async Task<ShoppingCartModel> GetShoppingCart(GetShoppingCartRequest request, ServerCallContext context)
        {
            var shoppingCart = await _shoppingCartContext.ShoppingCarts.FirstOrDefaultAsync(x => x.UserName == request.Username);

            if (shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with username = {request.Username} is not found"));
            }

            var shoppingCartModel = _mapper.Map<ShoppingCartModel>(shoppingCart);

            return shoppingCartModel;
        }

        public override async Task<ShoppingCartModel> CreateShoppingCart(ShoppingCartModel request, ServerCallContext context)
        {
            var shoppingCart = _mapper.Map<ShoppingCart>(request);

            var isExist = await _shoppingCartContext.ShoppingCarts.AnyAsync(x => x.UserName == request.Username);

            if (isExist)
            {
                _logger.LogError("Invalid UserName for ShoppingCart creation. UserName: {userName}", request.Username);

                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with username = {request.Username} is already exist"));
            }

            _shoppingCartContext.ShoppingCarts.Add(shoppingCart);
            await _shoppingCartContext.SaveChangesAsync();

            _logger.LogInformation("ShoppingCart is successfully created username: {userName}", request.Username);

            return _mapper.Map<ShoppingCartModel>(shoppingCart);
        }

        [AllowAnonymous]
        public override async Task<RemoveItemIntoShoppingCartResponse> RemoveItemIntoShoppingCart(RemoveItemIntoShoppingCartRequest request,
                                                                                                    ServerCallContext context)
        {
            //Get sc if exist or not
            //Check item if exist in sc or not
            //Remove item in SC db

            var shoppingCart = await _shoppingCartContext.ShoppingCarts.FirstOrDefaultAsync(x => x.UserName == request.Username);

            if (shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with username = {request.Username} is not found"));
            }

            var removeCartItem = shoppingCart.Items.FirstOrDefault(i => i.ProductId == request.RemoveCartItem.ProductId);

            if (removeCartItem == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"CartItem with ProductId= {request.RemoveCartItem.ProductId} is not found in the shoppingCart"));
            }

            shoppingCart.Items.Remove(removeCartItem);

            var removeCount = await _shoppingCartContext.SaveChangesAsync();

            return new RemoveItemIntoShoppingCartResponse
            {
                Success = removeCount > 0
            };
        }

        [AllowAnonymous]
        public override async Task<AddItemIntoShoppingCartResponse> AddItemIntoShoppingCart(IAsyncStreamReader<AddItemIntoShoppingCartRequest> requestStream,
                                                                                            ServerCallContext context)
        {
            //Get sc if exist or not
            //Check item if exist in sc or not
            //If item is exist +1 quantity
            //If item is not exist add new item into sc
            //Check discount and calculate the item price

            while (await requestStream.MoveNext())
            {
                var shoppingCart = await _shoppingCartContext.ShoppingCarts.FirstOrDefaultAsync(x => x.UserName == requestStream.Current.Username);

                if (shoppingCart == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with username = {requestStream.Current.Username} is not found"));
                }

                var newAddedCartItem = _mapper.Map<ShoppingCartItem>(requestStream.Current.NewCartItem);

                var cartItem = shoppingCart.Items.FirstOrDefault(i => i.ProductId == newAddedCartItem.ProductId);

                if (cartItem != null)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    //grpc call discount service -- check discount and calculate the item last price

                    var discount = await _discountService.GetDiscount(requestStream.Current.DiscountCode);

                    newAddedCartItem.Price -= discount.Amount;

                    shoppingCart.Items.Add(newAddedCartItem);
                }
            }

            var insertCount = await _shoppingCartContext.SaveChangesAsync();

            return new AddItemIntoShoppingCartResponse
            {
                InsertCount = insertCount,
                Success = insertCount > 0
            };
        }
    }
}

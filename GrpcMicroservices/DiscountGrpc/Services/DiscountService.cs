using DiscountGrpc.Data;
using DiscountGrpc.Protos;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscountGrpc
{
    public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
    {
        private readonly ILogger<DiscountService> _logger;

        public DiscountService(ILogger<DiscountService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<DiscountModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
        {
            var discount = DiscountContext.Discounts.FirstOrDefault(x => x.Code == request.DiscountCode);

            if (discount == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Discount with code = {request.DiscountCode} is not found"));
            }

            _logger.LogInformation("Discount is operated with the {discountCode} code and the amount is: {discountAmount}", discount.Code, discount.Amount);

            return await Task.FromResult(new DiscountModel
            {
                Amount = discount.Amount,
                Code = discount.Code,
                DiscountId = discount.DiscountId
            });

        }
    }
}

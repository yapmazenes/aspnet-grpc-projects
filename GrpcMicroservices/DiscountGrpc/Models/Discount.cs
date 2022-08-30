using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscountGrpc.Models
{
    public class Discount
    {
        public int DiscountId { get; set; }

        public string Code { get; set; }
        public float Amount { get; set; }
    }
}

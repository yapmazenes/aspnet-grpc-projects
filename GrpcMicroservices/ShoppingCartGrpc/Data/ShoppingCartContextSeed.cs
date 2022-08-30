﻿using ShoppingCartGrpc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingCartGrpc.Data
{
    public class ShoppingCartContextSeed
    {
        public static void Seed(ShoppingCartContext shoppingCartContext)
        {
            if (!shoppingCartContext.ShoppingCarts.Any())
            {
                var shoppingCart = new List<ShoppingCart>
                {
                      new ShoppingCart
                    {
                        UserName = "enes",
                        Items = new List<ShoppingCartItem>
                        {
                            new ShoppingCartItem
                            {
                                Quantity = 2,
                                Color ="Black",
                                Price = 699,
                                ProductId = 1,
                                ProductName = "Mi10T"
                            },
                            new ShoppingCartItem
                            {
                                Quantity = 3,
                                Color = "Red",
                                Price = 899,
                                ProductId = 2,
                                ProductName = "P40"
                            }
                        }
                    },

                };

                shoppingCartContext.ShoppingCarts.AddRange(shoppingCart);
                shoppingCartContext.SaveChanges();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground.Data
{
    public class ShoppingBasket
    {
        public string Id { get; set; }

        public Buyer Buyer { get; set; }
        public string BuyerId { get; set; }

        public List<ShoppingBasketItem> ShoppingBasketItems { get; set; }
    }
}

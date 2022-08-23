using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground.Data
{
    public class ShoppingBasketItem
    {
        public Item Item { get; set; }
        public int ItemId { get; set; }

        public ShoppingBasket ShoppingBasket { get; set; }
        public string ShoppingBasketId { get; set; }

    }
}

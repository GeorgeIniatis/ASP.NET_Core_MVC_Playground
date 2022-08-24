using ASP.NET_Core_MVC_Playground.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground.Models
{
    public class ItemAddedToBasketViewModel
    {
        public Item Item { get; set; }

        public bool AddedToBasket { get; set; }

    }
}

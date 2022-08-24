using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground.Data
{
    public class BoughtItem
    {
        public Buyer Buyer { get; set; }
        public string BuyerId { get; set; }
        public Item Item { get; set; }
        public int ItemId { get; set; }
        public DateTime DateBought { get; set; }
    }
}

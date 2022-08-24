using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground.Data
{
    public class Buyer
    {   
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }

        public ShoppingBasket ShoppingBasket { get; set; }

        public List<BoughtItem> BoughItems { get; set; }

        [Display(Name = "Total Spent")]
        public float? TotalSpent { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground.Data
{
    public class Borrower
    {   
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }

        [Display(Name = "Items Borrowed")]
        public List<Item> ItemsBorrowed { get; set; }

        [Display(Name = "Total Owed")]
        public float? TotalOwed { get; set; }
    }
}

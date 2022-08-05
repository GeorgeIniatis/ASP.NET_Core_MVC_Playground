using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASP.NET_Core_MVC_Playground.Data
{
    public class Item
    {
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "NameIsRequired", ErrorMessageResourceType = typeof(AppResources.ItemModel))]
        [Display(Name = "Name", ResourceType = typeof(AppResources.ItemModel))]
        public string Name { get; set; }

        [Required(ErrorMessageResourceName = "PriceIsRequired", ErrorMessageResourceType = typeof(AppResources.ItemModel))]
        [Display(Name = "Price", ResourceType = typeof(AppResources.ItemModel))]
        [Range(0, float.MaxValue, ErrorMessage = "Please enter a positive price")]
        public float Price { get; set; }

        public byte[] ImageBytes { get; set; }

        [Display(Name = "Description", ResourceType = typeof(AppResources.ItemModel))]
        public string Description { get; set; }

        [Display(Name = "Owner", ResourceType = typeof(AppResources.ItemModel))]
        public string OwnerID { get; set; }
        public Owner Owner { get; set; }


        [Display(Name = "Borrower", ResourceType = typeof(AppResources.ItemModel))]
#nullable enable
        public string? BorrowerID { get; set; }
#nullable disable
        public Borrower Borrower { get; set; }

        [Display(Name = "DataBorrowed", ResourceType = typeof(AppResources.ItemModel))]
        public DateTime? BorrowedDate { get; set; }



    }
}

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
        [Range(0, long.MaxValue, ErrorMessage = "Please enter a positive price")]
        public long Price { get; set; }

        public byte[] ImageBytes { get; set; }

        [Required(ErrorMessageResourceName = "StripeImageUrlIsRequired", ErrorMessageResourceType = typeof(AppResources.ItemModel))]
        public string StripeImageUrl { get; set; }

        [Display(Name = "Description", ResourceType = typeof(AppResources.ItemModel))]
        public string Description { get; set; }

        [Display(Name = "Seller", ResourceType = typeof(AppResources.ItemModel))]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }


        [Display(Name = "Buyer", ResourceType = typeof(AppResources.ItemModel))]
#nullable enable
        public string? BuyerId { get; set; }
#nullable disable
        public Buyer Buyer { get; set; }

        [Display(Name = "DateBought", ResourceType = typeof(AppResources.ItemModel))]
        public DateTime? DateBought { get; set; }



    }
}

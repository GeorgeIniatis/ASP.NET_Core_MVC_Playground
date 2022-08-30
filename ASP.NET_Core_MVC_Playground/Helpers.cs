using ASP.NET_Core_MVC_Playground.Areas.Identity.Data;
using ASP.NET_Core_MVC_Playground.Controllers;
using ASP.NET_Core_MVC_Playground.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground
{
    public class StripeOptions
    {
        public string? PublishableKey { get; set; }
        public string? SecretKey { get; set; }
        public string? EndpointSecretKey { get; set; }
    }

    public class AppOptions
    {
        public string? Url { get; set; }
    }

    public class Helpers
    {
        private readonly DataDbContext _db;
        private readonly ILogger _logger;
        private StripeOptions StripeOptions { get; }
        private AppOptions AppOptions { get; }
        private readonly UserManager<ApplicationUser> _userManager;

        public Helpers(DataDbContext db, 
                       ILogger<Helpers> logger, 
                       IOptions<StripeOptions> optionsAccessorStripe,
                       IOptions<AppOptions> optionsAccessorApp,
                       UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _logger = logger;
            StripeOptions = optionsAccessorStripe.Value;
            AppOptions = optionsAccessorApp.Value;
            _userManager = userManager;
        }

        public async Task createNewBuyer(ApplicationUser User)
        {

        }

        public async Task<Item> getItemObject(int Id)
        {
            Item item = await (from items in _db.Items
                               where items.Id == Id
                               select items).FirstOrDefaultAsync();
            return item;
        }

        public void saveItem(Item item)
        {
            _db.Items.Add(item);
            _db.SaveChanges();
        }

        public async Task updateItem(Item item)
        {
            _db.Items.Update(item);
            if (await _db.SaveChangesAsync() > 0)
            {
                return;
            }
        }

        public void removeItem(Item item)
        {
            _db.Remove(item);
            _db.SaveChanges();
        }

        public List<SelectListItem> getSellersAsSelectListItems()
        {
            List<SelectListItem> sellers = new();
            sellers.Add(new SelectListItem { Value = String.Empty, Text = "Seller" });

            foreach (Seller seller in _db.Sellers)
            {
                sellers.Add(new SelectListItem { Value = seller.Id.ToString(), Text = seller.FullName });
            }

            return sellers;
        }

        public async Task<string> getUserShoppingBasketId(ClaimsPrincipal User)
        {
            string userId = _userManager.GetUserId(User);

            string userShoppingBasketId = await (from buyers in _db.Buyers.Include(i => i.ShoppingBasket)
                                                 where buyers.Id == userId
                                                 select buyers.ShoppingBasket.Id).FirstAsync();
            return userShoppingBasketId;
        }

        public async Task<Buyer> getBuyerFromUser(ClaimsPrincipal User)
        {
            string userId = _userManager.GetUserId(User);
            Buyer buyer = await (from buyers in _db.Buyers
                                 where buyers.Id == userId
                                 select buyers).FirstOrDefaultAsync();
            return buyer;
        }

        private byte[] processImage(IFormFile ImageFile)
        {
            if (ImageFile != null)
            {
                if ((ImageFile.Length > 0) & (ImageFile.ContentType.Split("/")[0] == "image"))
                {
                    var image = Image.FromStream(ImageFile.OpenReadStream());
                    var resized = new Bitmap(image, new Size(256, 256));
                    using (var imageStream = new MemoryStream())
                    {
                        resized.Save(imageStream, ImageFormat.Jpeg);
                        return imageStream.ToArray();
                    }
                }
            }
            return null;
        }

        private string processTextFile(IFormFile TextFile)
        {
            if (TextFile != null)
            {
                if ((TextFile.Length > 0) & (TextFile.ContentType == "text/plain"))
                {
                    using (var streamReader = new StreamReader(TextFile.OpenReadStream()))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
            return null;
        }

        public string[] createStripeProduct(Item item)
        {
            StripeConfiguration.ApiKey = StripeOptions.SecretKey;

            var options = new ProductCreateOptions
            {
                Name = item.Name,
                Description = item.Description,
                DefaultPriceData = new ProductDefaultPriceDataOptions
                {
                    Currency = "eur",
                    UnitAmount = item.Price * 100,
                },
                Images = new List<String>() { item.StripeImageUrl },
            };
            var service = new ProductService();
            Product productCreated = service.Create(options);
            return new string[] { productCreated.Id, productCreated.DefaultPriceId };
        }

        public void editStripeProduct(Item item)
        {
            bool priceChanged = (from items in _db.Items
                                 where items.Id == item.Id
                                 select items.Price).First() != item.Price;

            string oldPriceToArchive = null;
            if(priceChanged)
            {
                oldPriceToArchive = item.StripePriceId;
                item.StripePriceId = createStripePrice(item);
            }

            StripeConfiguration.ApiKey = StripeOptions.SecretKey;

            var options = new ProductUpdateOptions 
            { 
                Name = item.Name,
                Description = item.Description,
                DefaultPrice = item.StripePriceId,
                Images = new List<String>() { item.StripeImageUrl },
            };
            var service = new ProductService();
            service.Update(item.StripeId, options);

            if(oldPriceToArchive != null)
            {
                archiveOldPrice(oldPriceToArchive);
            }
        }

        public string createStripePrice(Item item)
        {
            StripeConfiguration.ApiKey = StripeOptions.SecretKey;

            var createOptions = new PriceCreateOptions
            {
                UnitAmount = item.Price * 100,
                Currency = "eur",
                Product = item.StripeId
            };
            var createService = new PriceService();
            return createService.Create(createOptions).Id; 
        }

        public void archiveOldPrice(string oldPriceToArchive)
        {
            StripeConfiguration.ApiKey = StripeOptions.SecretKey;

            var updateOptions = new PriceUpdateOptions
            {
                Active = false
            };
            var updateService = new PriceService();
            updateService.Update(oldPriceToArchive, updateOptions);
        }

        public void archiveStripeProduct(Item item)
        {
            StripeConfiguration.ApiKey = StripeOptions.SecretKey;

            // Update Stripe Product Price Object to NULL
            var options = new ProductUpdateOptions
            {
                Active = false,
            };
            var updateService = new ProductService();
            updateService.Update(item.StripeId, options);
        }

        public string returnStripeEndpointKey()
        {
            return StripeOptions.EndpointSecretKey;
        }

        public string returnUrl()
        {
            return AppOptions.Url;
        }
    }
}

using ASP.NET_Core_MVC_Playground.Controllers;
using ASP.NET_Core_MVC_Playground.Data;
using Microsoft.AspNetCore.Http;
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
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground
{
    public class StripeOptions
    {
        public string? PublishableKey { get; set; }
        public string? SecretKey { get; set; }
    }

    public class Helpers
    {
        private readonly DataDbContext _db;
        private readonly ILogger _logger;
        private StripeOptions StripeOptions { get; }

        public Helpers(DataDbContext db, ILogger<Helpers> logger, IOptions<StripeOptions> optionsAccessorStripe)
        {
            _db = db;
            _logger = logger;
            StripeOptions = optionsAccessorStripe.Value;
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

        public void createStripeProduct(Item item)
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
            service.Create(options);
        }
    }
}

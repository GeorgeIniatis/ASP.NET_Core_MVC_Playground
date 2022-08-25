using ASP.NET_Core_MVC_Playground.Areas.Identity.Data;
using ASP.NET_Core_MVC_Playground.Data;
using ASP.NET_Core_MVC_Playground.Models;
using Microsoft.AspNetCore.Authorization;
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
using System.Text;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground.Controllers
{
    public class ItemController : Controller
    {
        private readonly DataDbContext _db;
        private readonly ILogger _logger;
        private readonly Helpers helpers;
        
        public ItemController(DataDbContext db, 
                              ILogger<ItemController> logger, 
                              Helpers Helpers)
        {
            _db = db;
            _logger = logger;
            helpers = Helpers;
        }

        // Alternative Way
        // Instead of ASP.NET_Core_MVC_Playground.Controllers.ItemController
        // Replace it with DemoCategory
        //public ItemController(DataDbContext db, ILoggerFactory factory)
        //{
        //    _db = db;
        //    _logger = factory.CreateLogger("DemoCategory");
        //}


        // Public Accessible Actions
        public async Task<IActionResult> Index(string searchString)
        {
            List<Item> itemList = await _db.Items.Include(i => i.Seller).ToListAsync();
            
            if(!String.IsNullOrEmpty(searchString))
            {
                itemList = await (from items in _db.Items
                                  where items.Name.Contains(searchString)
                                  select items).Include(i => i.Seller).ToListAsync();
            }

            List<ItemAddedToBasketViewModel> viewModelsList = new();

            string userShoppingBasketId = await helpers.getUserShoppingBasketId(User);

            foreach (Item item in itemList)
            {
                ItemAddedToBasketViewModel newViewModel = new()
                {
                    Item = item,
                    AddedToBasket = await (from shoppingBasketItems in _db.ShoppingBasketItems
                                           where shoppingBasketItems.ShoppingBasketId == userShoppingBasketId && shoppingBasketItems.ItemId == item.Id
                                           select shoppingBasketItems).AnyAsync()
                };
                viewModelsList.Add(newViewModel);
            }

            ViewBag.Status = TempData["Message"];
            return View(viewModelsList);
        }

        public async Task<IActionResult> Checkout()
        {
            string userShoppingBasketId = await helpers.getUserShoppingBasketId(User);

            List<Item> itemsInShoppingBasket = await (from shoppingBasketItems in _db.ShoppingBasketItems.Include(i => i.Item)
                                                        where shoppingBasketItems.ShoppingBasketId == userShoppingBasketId
                                                        select shoppingBasketItems.Item).ToListAsync();

            return View(itemsInShoppingBasket);
        }

        public async Task<IActionResult> AddToBasket(int id)
        {
            Item item = await (from items in _db.Items
                               where items.Id == id
                               select items).FirstOrDefaultAsync();

            if (item == null)
            {
                return RedirectToAction("Index");
            }

            string userShoppingBasketId = await helpers.getUserShoppingBasketId(User);
            ShoppingBasket shoppingBasket = await (from shoppingBaskets in _db.ShoppingBaskets
                                                   where shoppingBaskets.Id == userShoppingBasketId
                                                   select shoppingBaskets).FirstOrDefaultAsync();

            ShoppingBasketItem newShoppingBasketItem = new()
            {
                Item = item,
                ItemId = id,
                ShoppingBasket = shoppingBasket,
                ShoppingBasketId = userShoppingBasketId,
            };

            _db.ShoppingBasketItems.Add(newShoppingBasketItem);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RemoveFromBasket(int id)
        {
            string userShoppingBasketId = await helpers.getUserShoppingBasketId(User);
            ShoppingBasketItem itemInBasket = await (from shoppingBasketItems in _db.ShoppingBasketItems
                                                     where shoppingBasketItems.ShoppingBasketId == userShoppingBasketId && shoppingBasketItems.ItemId == id
                                                     select shoppingBasketItems).FirstOrDefaultAsync();

            if (itemInBasket != null)
            {
                _db.ShoppingBasketItems.Remove(itemInBasket);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Checkout");
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Cancel()
        {
            return View();
        }


        // Admin Accessible Actions
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            List<SelectListItem> sellers = helpers.getSellersAsSelectListItems();

            ViewBag.sellers = sellers;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(ItemImageViewModel model, IFormFile ImageFile, IFormFile TextFile)
        {
            if(ModelState.IsValid)
            {
                // Used for IFormFile ImageFile Example
                //byte[] imageBytes = processImage(ImageFile);

                // Used for IFormFile TextFile Example
                //model.Item.Description = processTextFile(TextFile);
                if (model.ImageFile != null)
                {
                    string imageFileBase64 = model.ImageFile;
                    byte[] imageBytes = Convert.FromBase64String(imageFileBase64.Split(',')[1]);
                    model.Item.ImageBytes = imageBytes;
                }
                else
                {
                    string errorMessage = "Item could not be created. Image was not provided";
                    ViewBag.Status = errorMessage;
                    _logger.LogError(errorMessage);
                    return View();
                }
                
                if (model.TextFile != null)
                {
                    string textFileBase64 = model.TextFile;
                    byte[] textBytes = Convert.FromBase64String(textFileBase64.Split(',')[1]);
                    model.Item.Description = Encoding.UTF8.GetString(textBytes);
                }
                else
                {
                    string errorMessage = "Item could not be created. Description was not provided";
                    ViewBag.Status = errorMessage;
                    _logger.LogError(errorMessage);
                    return View();
                }
                string[] newProductCreated = helpers.createStripeProduct(model.Item);
                model.Item.StripeId = newProductCreated[0];
                model.Item.StripePriceId = newProductCreated[1];
                helpers.saveItem(model.Item);

                TempData["Message"] = "Item created successfully!";
                _logger.LogInformation("Item Created with Name:{ItemName}",model.Item.Name);
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Status = "Item could not be created!";
                _logger.LogError("Item could not be created!");
                return View();
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int Id)
        {
            ItemImageViewModel model = new();
            Item item = await helpers.getItemObject(Id);
            model.Item = item;

            if (item != null)
            {
                List<SelectListItem> sellers = new();
                sellers.Add(new SelectListItem { Value = String.Empty, Text = "Seller" });

                foreach (Seller seller in _db.Sellers)
                {
                    var selected = false;
                    if (seller == item.Seller)
                    {
                        selected = true;
                    }
                    sellers.Add(new SelectListItem { Value = seller.Id.ToString(), Text = seller.FullName, Selected = selected });

                }
                ViewBag.sellers = sellers;

                return View(model);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        //[Bind("Id,Name,Price,SellerId,BuyerId")]
        public async Task<IActionResult> Edit(int Id,  ItemImageViewModel model, IFormFile ImageFile, IFormFile TextFile)
        {
            if (Id != model.Item.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (model.Item.SellerId == null)
                {
                    TempData["Message"] = "The Item needs to have a Seller!";
                    return RedirectToAction("Index");
                }

                // Used for IFormFile Example
                // byte[] imageBytes = processImage(ImageFile);
                if (model.ImageFile != null)
                {
                    string imageFileBase64 = model.ImageFile;
                    byte[] imageBytes = Convert.FromBase64String(imageFileBase64.Split(',')[1]);
                    model.Item.ImageBytes = imageBytes;
                }

                // Used for IFormFile Example
                // string description = processTextFile(TextFile);
                
                if (model.TextFile != null)
                {
                    string textFileBase64 = model.TextFile;
                    byte[] textBytes = Convert.FromBase64String(textFileBase64.Split(',')[1]);
                    model.Item.Description = Encoding.UTF8.GetString(textBytes);
                }

                try
                {
                    helpers.editStripeProduct(model.Item);
                    await helpers.updateItem(model.Item);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogInformation(ex, "Exception when trying to update item {ItemName}", model.Item.Name);
                    if (await helpers.getItemObject(model.Item.Id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["Message"] = "Item edited successfully!";
            }
            else
            {
                TempData["Message"] = "Item could not be edited!";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Remove(int Id)
        {
            Item item = await helpers.getItemObject(Id);

            if (item != null)
            {
                helpers.archiveStripeProduct(item);
                helpers.removeItem(item);
                TempData["Message"] = "Item removed successfully!";
            }
            else
            {
                TempData["Message"] = "Item could not be removed!";
            }
            return RedirectToAction("Index");
        }

        // Helper Functions
        public FileContentResult getImg(int itemId)
        {
            byte[] byteArray = (from items in _db.Items
                                where items.Id == itemId
                                select items.ImageBytes).FirstOrDefault();

            if (byteArray != null)
            {
                return new FileContentResult(byteArray, "image/jpeg");
            }
            return null;
        }
    }
}

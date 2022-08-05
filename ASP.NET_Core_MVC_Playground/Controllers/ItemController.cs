using ASP.NET_Core_MVC_Playground.Data;
using ASP.NET_Core_MVC_Playground.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground.Controllers
{
    public class ItemController : Controller
    {
        private readonly DataDbContext _db;
        private readonly ILogger _logger;

        public ItemController(DataDbContext db, ILogger<ItemController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // Alternative Way
        // Instead of ASP.NET_Core_MVC_Playground.Controllers.ItemController
        // Replace it with DemoCategory
        //public ItemController(DataDbContext db, ILoggerFactory factory)
        //{
        //    _db = db;
        //    _logger = factory.CreateLogger("DemoCategory");
        //}

        public async Task<IActionResult> Index(string searchString)
        {
            IEnumerable<Item> itemList = _db.Items.Include(i => i.Owner).Include(i => i.Borrower);

            if(!String.IsNullOrEmpty(searchString))
            {
                itemList = await (from items in _db.Items
                                  where items.Name.Contains(searchString)
                                  select items).Include(i => i.Owner).Include(i => i.Borrower).ToListAsync();
            }

            ViewBag.Status = TempData["Message"];
            return View(itemList);
        }

        public IActionResult Create()
        {
            List<SelectListItem> owners = getOwnersAsSelectListItems();

            ViewBag.owners = owners;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                
                saveItem(model.Item);

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

        public async Task<IActionResult> Edit(int Id)
        {
            ItemImageViewModel model = new();
            Item item = await getItemObject(Id);
            model.Item = item;

            if (item != null)
            {
                List<SelectListItem> owners = new();
                owners.Add(new SelectListItem { });

                foreach (Owner owner in _db.Owners)
                {
                    var selected = false;
                    if (owner == item.Owner)
                    {
                        selected = true;
                    }
                    owners.Add(new SelectListItem { Value = owner.Id.ToString(), Text = owner.FullName, Selected = selected });

                }

                List<SelectListItem> borrowers = new();
                borrowers.Add(new SelectListItem { });

                foreach (Borrower borrower in _db.Borrowers)
                {
                    var selected = false;
                    if (borrower == item.Borrower)
                    {
                        selected = true; 
                    }
                    borrowers.Add(new SelectListItem { Value = borrower.Id.ToString(), Text = borrower.FullName, Selected = selected });
                }

                ViewBag.owners = owners;
                ViewBag.borrowers = borrowers;

                return View(model);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Bind("Id,Name,Price,OwnerID,BorrowerID")]
        public async Task<IActionResult> Edit(int Id,  ItemImageViewModel model, IFormFile ImageFile, IFormFile TextFile)
        {
            if (Id != model.Item.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (model.Item.OwnerID == null)
                {
                    TempData["Message"] = "The Item needs to have an Owner!";
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

                if (model.Item.BorrowerID != null)
                {
                    model.Item.BorrowedDate = DateTime.Now;
                }

                try
                {
                    await updateItem(model.Item);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogInformation(ex, "Exception when trying to update item {ItemName}", model.Item.Name);
                    if (getItemObject(model.Item.Id) == null)
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
            Item item = await getItemObject(Id);

            if (item != null)
            {
                removeItem(item);
                TempData["Message"] = "Item removed successfully!";
            }
            else
            {
                TempData["Message"] = "Item could not be removed!";
            }
            return RedirectToAction("Index");
        }

        // Helper Functions
        private async Task<Item> getItemObject(int Id)
        {
            Item item = await (from items in _db.Items
                               where items.Id == Id
                               select items).FirstOrDefaultAsync();
            return item;
        }

        private void saveItem(Item item)
        {
            _db.Items.Add(item);
            _db.SaveChanges();
        }

        private async Task updateItem(Item item)
        {
            _db.Items.Update(item);   
            if (await _db.SaveChangesAsync() > 0)
            {
                return;
            }
        }

        private void removeItem(Item item)
        {
            _db.Remove(item);
            _db.SaveChanges();
        }

        private List<SelectListItem> getOwnersAsSelectListItems()
        {
            List<SelectListItem> owners = new();
            owners.Add(new SelectListItem { Value = String.Empty, Text= "Owner"});

            foreach (Owner owner in _db.Owners)
            {
                owners.Add(new SelectListItem { Value = owner.Id.ToString(), Text = owner.FullName });
            }

            return owners;
        }

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
            if(TextFile != null)
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
    }
}

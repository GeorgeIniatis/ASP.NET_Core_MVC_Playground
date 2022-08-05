using ASP.NET_Core_MVC_Playground.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground.Controllers
{
    public class TinyController : Controller
    {
        private readonly DataDbContext _db;
        private readonly ILogger _logger;

        public TinyController(DataDbContext db, ILogger<TinyController> logger)
        {
            _db = db;
            _logger = logger; 
        }
        public IActionResult Setup()
        {
            Tiny model = _db.Tinies.Find("DynamicPage");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Setup(Tiny model)
        {
            if (ModelState.IsValid)
            {
                if (model.Page != null)
                {
                    _db.Tinies.Update(model);
                }
                else
                {
                    model.Page = "DynamicPage";
                    _db.Tinies.Add(model);
                }
                _db.SaveChanges();
            }
            return View(model);
        }

        public IActionResult DynamicPage()
        {
            Tiny model = _db.Tinies.Find("DynamicPage");
            return View(model);
        }
    }
}

using ASP.NET_Core_MVC_Playground.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground.Controllers
{
    [Route("create-checkout-session")]
    [ApiController]
    public class CheckoutApiController : Controller
    {
        private readonly DataDbContext _db;
        private readonly ILogger _logger;
        private readonly Helpers helpers;

        public CheckoutApiController(DataDbContext db, 
                                     ILogger<CheckoutApiController> logger,
                                     Helpers Helpers)
        {
            _db = db;
            _logger = logger;
            helpers = Helpers;
        }


        [HttpPost]
        public async Task<IActionResult> Create()
        {
            string userShoppingBasketId = await helpers.getUserShoppingBasketId(User);

            List<Item> itemsInShoppingBasket = await(from shoppingBasketItems in _db.ShoppingBasketItems.Include(i => i.Item)
                                                     where shoppingBasketItems.ShoppingBasketId == userShoppingBasketId
                                                     select shoppingBasketItems.Item).ToListAsync();

            List<SessionLineItemOptions> sessionLineItemOptionsList = new();
            foreach(Item item in itemsInShoppingBasket)
            {
                sessionLineItemOptionsList.Add(new SessionLineItemOptions
                {
                    Price = item.StripePriceId,
                    Quantity = 1,
                });
            }

            var domain = helpers.returnUrl();
            var options = new SessionCreateOptions
            {
                LineItems = sessionLineItemOptionsList,
                Mode = "payment",
                SuccessUrl = domain + "Item/Success",
                CancelUrl = domain + "Item/Cancel",
            };
            var service = new SessionService();
            Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
    }
}

using ASP.NET_Core_MVC_Playground.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground.Controllers
{
    [Route("webhook")]
    [ApiController]
    public class WebhookController : Controller
    {
        private readonly DataDbContext _db;
        private readonly ILogger _logger;
        private readonly Helpers helpers;

        public WebhookController(DataDbContext db,
                                 ILogger<CheckoutApiController> logger,
                                 Helpers Helpers)
        {
            _db = db;
            _logger = logger;
            helpers = Helpers;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            string endpointSecret = helpers.returnStripeEndpointKey();

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], endpointSecret);

                // Handle the event
                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    _logger.LogInformation("Stripe: Payment Successful");
                }
                else
                {
                    _logger.LogError("Stripe: {0} was trigerred", stripeEvent.Type);
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ASP.NET_Core_MVC_Playground.Data
{
    public class SeedData
    {
        private readonly ILogger _logger;

        public SeedData(ILogger<SeedData> logger)
        {
            _logger = logger;
        }

        public void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new DataDbContext(serviceProvider.GetRequiredService<DbContextOptions<DataDbContext>>()))
            {
                populateSellers(context);
                populateItems(context);
                populateBuyers(context);
                populateItemPurchases(context);
            }
        }

        private void populateSellers(DataDbContext context)
        {
            if (context.Sellers.Any())
            {
                _logger.LogInformation("Sellers already seeded!");
                return;
            }

            context.Sellers.AddRange(
                    new Seller
                    {
                        FirstName = "George",
                        LastName = "Iniatis",
                        PhoneNumber = 99355566,
                        Email = "jogeo98@hotmail.com"
                    },
                    new Seller
                    {
                        FirstName = "Bill",
                        LastName = "Adama",
                        PhoneNumber = 99643155,
                        Email = "antonis@summecliff.com"
                    }
                );

            context.SaveChanges();
            _logger.LogInformation("Sellers seeded!");
        }

        private void populateItems(DataDbContext context)
        {
            if (context.Items.Any())
            {
                _logger.LogInformation("Items already seeded!");
                return;
            }
            
            context.Items.AddRange(
                new Item
                {
                    Name = "Xbox",
                    Price = 400,
                    SellerId = (from sellers in context.Sellers
                                where sellers.FullName == "George Iniatis"
                                select sellers.Id).First(),
                    Description = "Xbox Series X Console"
                },
                new Item
                {
                    Name = "Phone",
                    Price = 600,
                    SellerId = (from sellers in context.Sellers
                                where sellers.FullName == "George Iniatis"
                                select sellers.Id).First(),
                    Description = "Galaxy S21 5G Smartphone"
                },
                new Item
                {
                    Name = "Battlestar Galactica",
                    Price = 15000,
                    SellerId = (from sellers in context.Sellers
                                where sellers.FullName == "Bill Adama"
                                select sellers.Id).First(),
                    Description = "The pride of the colonial fleet"
                },
                new Item
                {
                    Name = "Battlestar Pegasus",
                    Price = 30000,
                    SellerId = (from sellers in context.Sellers
                                where sellers.FullName == "Bill Adama"
                                select sellers.Id).First(),
                    Description = "Improved battlestar but lacking in leadership"
                }
            );

            context.SaveChanges();
            _logger.LogInformation("Items seeded!");
        }

        private void populateBuyers(DataDbContext context)
        {
            if (context.Buyers.Any())
            {
                _logger.LogInformation("Buyers already seeded!");
                return;
            }

            context.Buyers.AddRange(
                new Buyer
                {
                    FirstName = "Saul",
                    LastName = "Tigh"
                },
                new Buyer
                {
                    FirstName = "Helena",
                    LastName = "Cain"
                }
            );
            context.SaveChanges();
            _logger.LogInformation("Buyers seeded!");
        }

        private void populateItemPurchases(DataDbContext context)
        {
            var anyBorrowings = (from items in context.Items
                                 where items.BuyerId != null
                                 select items).Any();
            if(!anyBorrowings)
            {
                Item galactica = (from items in context.Items
                             where items.Name == "Battlestar Galactica"
                             select items).First();

                galactica.BuyerId = (from buyers in context.Buyers
                                     where buyers.FullName == "Saul Tigh"
                                     select buyers.Id).First();

                galactica.DateBought = DateTime.Now;

                context.Items.Update(galactica);

                Item pegasus = (from items in context.Items
                                where items.Name == "Battlestar Pegasus"
                                select items).First();

                pegasus.BuyerId = (from buyers in context.Buyers
                                   where buyers.FullName == "Helena Cain"
                                   select buyers.Id).First();

                pegasus.DateBought = DateTime.Now;

                context.Items.Update(pegasus);

                context.SaveChanges();
                _logger.LogInformation("Item Borrowings seeded!");

            }
        }
    }
}

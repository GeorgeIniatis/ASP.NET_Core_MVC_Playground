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
                populateOwners(context);
                populateItems(context);
                populateBorrowers(context);
                populateItemsBorrowing(context);
            }
        }

        private void populateOwners(DataDbContext context)
        {
            if (context.Owners.Any())
            {
                _logger.LogInformation("Owners already seeded!");
                return;
            }

            context.Owners.AddRange(
                    new Owner
                    {
                        FirstName = "George",
                        LastName = "Iniatis",
                        PhoneNumber = 99355566,
                        Email = "jogeo98@hotmail.com"
                    },
                    new Owner
                    {
                        FirstName = "Bill",
                        LastName = "Adama",
                        PhoneNumber = 99643155,
                        Email = "antonis@summecliff.com"
                    }
                );

            context.SaveChanges();
            _logger.LogInformation("Owners seeded!");
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
                    OwnerID = (from owners in context.Owners
                             where owners.FullName == "George Iniatis"
                             select owners.Id).First(),
                    Description = "Xbox Series X Console"
                },
                new Item
                {
                    Name = "Phone",
                    Price = 600,
                    OwnerID = (from owners in context.Owners
                             where owners.FullName == "George Iniatis"
                             select owners.Id).First(),
                    Description = "Galaxy S21 5G Smartphone"
                },
                new Item
                {
                    Name = "Battlestar Galactica",
                    Price = 15000,
                    OwnerID = (from owners in context.Owners
                             where owners.FullName == "Bill Adama"
                             select owners.Id).First(),
                    Description = "The pride of the colonial fleet"
                },
                new Item
                {
                    Name = "Battlestar Pegasus",
                    Price = 30000,
                    OwnerID = (from owners in context.Owners
                             where owners.FullName == "Bill Adama"
                             select owners.Id).First(),
                    Description = "Improved battlestar but lacking in leadership"
                }
            );

            context.SaveChanges();
            _logger.LogInformation("Items seeded!");
        }

        private void populateBorrowers(DataDbContext context)
        {
            if (context.Buyers.Any())
            {
                _logger.LogInformation("Borrowers already seeded!");
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
            _logger.LogInformation("Borrowers seeded!");
        }

        private void populateItemsBorrowing(DataDbContext context)
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

                galactica.BorrowedDate = DateTime.Now;

                context.Items.Update(galactica);

                Item pegasus = (from items in context.Items
                             where items.Name == "Battlestar Pegasus"
                             select items).First();

                pegasus.BuyerId = (from buyers in context.Buyers
                                      where buyers.FullName == "Helena Cain"
                                      select buyers.Id).First();

                pegasus.BorrowedDate = DateTime.Now;

                context.Items.Update(pegasus);

                context.SaveChanges();
                _logger.LogInformation("Item Borrowings seeded!");

            }
        }
    }
}

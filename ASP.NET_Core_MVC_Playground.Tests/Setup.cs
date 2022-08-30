using ASP.NET_Core_MVC_Playground.Data;
using ASP.NET_Core_MVC_Playground.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground.Tests
{
    public class Setup : IEnumerable<object[]>, IDisposable
    {
        private readonly DataDbContext mockContext;

        public Setup()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataDbContext>();
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            mockContext = new DataDbContext(optionsBuilder.Options);

            // Add Sellers
            List<Seller> sellers = new()
            {
                new Seller()
                {
                    Id = "OwnerID",
                    FirstName = "John",
                    LastName = "Smith"
                },

                new Seller()
                {
                    Id = "OwnerID2",
                    FirstName = "John",
                    LastName = "Smith",
                }
            };

            List<Buyer> buyers = new()
            {
                new Buyer()
                {
                    Id = "Buyer",
                    FirstName = "Buyer",
                    LastName = "McBuyer"
                },

                new Buyer()
                {
                    Id = "Buyer2",
                    FirstName = "Buyer2",
                    LastName = "McBuyer"
                }
            };

            // Add Items
            List<Item> items = new()
            {
                new Item{
                    Name = "TestItem 1",
                    Price = 20,
                    Description = "Test Description 1",
                    SellerId = "OwnerID",
                    StripeImageUrl = " "
                },
                new Item{
                    Name = "TestItem 2",
                    Price = 220,
                    Description = "Test Description 2",
                    SellerId = "OwnerID2",
                    StripeImageUrl = " "
                }
            };

            mockContext.AddRange(sellers);
            mockContext.AddRange(buyers);
            mockContext.AddRange(items);
            mockContext.SaveChanges();
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { mockContext };
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            mockContext.Database.EnsureDeleted();
            mockContext.Dispose();
        }
    }
}

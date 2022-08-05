using ASP.NET_Core_MVC_Playground.Data;
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

            // Add Owners
            List<Owner> owners = new()
            {
                new Owner()
                {
                    Id = "OwnerID",
                    FirstName = "John",
                    LastName = "Smith"
                },

                new Owner()
                {
                    Id = "OwnerID2",
                    FirstName = "John",
                    LastName = "Smith",
                }
            };

            List<Borrower> borrowers = new()
            {
                new Borrower()
                {
                    Id = "Borrower",
                    FirstName = "Borrower",
                    LastName = "McBorrower"
                },

                new Borrower()
                {
                    Id = "Borrower2",
                    FirstName = "Borrower2",
                    LastName = "McBorrower"
                }
            };

            // Add Items
            List<Item> items = new List<Item>()
            {
                new Item{
                    Name = "TestItem 1",
                    Price = 20,
                    Description = "Test Description 1",
                    OwnerID = "OwnerID",
                    BorrowerID = "Borrower"
                },
                new Item{
                    Name = "TestItem 2",
                    Price = 220,
                    Description = "Test Description 2",
                    OwnerID = "OwnerID2",
                    BorrowerID = "Borrower2"
                }
            };

            mockContext.AddRange(owners);
            mockContext.AddRange(borrowers);
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

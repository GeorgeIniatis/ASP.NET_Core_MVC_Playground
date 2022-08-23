using ASP.NET_Core_MVC_Playground.Models;
using ASP.NET_Core_MVC_Playground.Data;
using System;
using Xunit;
using System.Collections.Generic;
using System.Collections;

namespace ASP.NET_Core_MVC_Playground.Tests
{
    public class ValidOwnerTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            Seller seller1 = new()
            {
                Id = "OwnerID",
                FirstName = "John",
                LastName = "Smith"
            };

            List<Item> items = new List<Item>()
            {
                new Item{
                    Name = "TestItem 1",
                    Price = 20,
                    Description = "Test Description 1",
                    SellerId = seller1.Id,
                },
                new Item{
                    Name = "TestItem 2",
                    Price = 220,
                    Description = "Test Description 2",
                    SellerId = seller1.Id,
                }
            };

            Seller seller2 = new()
            {
                Id = "OwnerID",
                FirstName = "John",
                LastName = "Smith",
                ItemsOwned = items
            };

            yield return new object[] { seller1 };
            yield return new object[] { seller2 };
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class TestSeller
    {
        [Theory]
        [ClassData(typeof(ValidOwnerTestData))]
        public void ValidOwnerIsNotNull(Seller seller)
        {
            Assert.NotNull(seller);
        }

        [Theory]
        [ClassData(typeof(ValidOwnerTestData))]
        public void ValidOwnerIsTypeOwner(Seller seller)
        {
            Assert.IsType<Seller>(seller);
        }
    }
}

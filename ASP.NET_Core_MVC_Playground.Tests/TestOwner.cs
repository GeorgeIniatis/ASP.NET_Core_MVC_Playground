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
            Owner owner1 = new()
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
                    OwnerID = owner1.Id,
                },
                new Item{
                    Name = "TestItem 2",
                    Price = 220,
                    Description = "Test Description 2",
                    OwnerID = owner1.Id,
                }
            };

            Owner owner2 = new()
            {
                Id = "OwnerID",
                FirstName = "John",
                LastName = "Smith",
                ItemsOwned = items
            };

            yield return new object[] { owner1 };
            yield return new object[] { owner2 };
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class TestOwner
    {
        [Theory]
        [ClassData(typeof(ValidOwnerTestData))]
        public void ValidOwnerIsNotNull(Owner owner)
        {
            Assert.NotNull(owner);
        }

        [Theory]
        [ClassData(typeof(ValidOwnerTestData))]
        public void ValidOwnerIsTypeOwner(Owner owner)
        {
            Assert.IsType<Owner>(owner);
        }
    }
}

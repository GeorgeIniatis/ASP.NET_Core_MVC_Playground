using ASP.NET_Core_MVC_Playground.Controllers;
using ASP.NET_Core_MVC_Playground.Data;
using ASP.NET_Core_MVC_Playground.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace ASP.NET_Core_MVC_Playground.Tests
{
    public class TestItemController
    {

        public ItemController getController(DataDbContext context)
        {
            var mockOptions = Options.Create(new StripeOptions());
            var mockHelper = new Helpers(context, new Logger<Helpers>(new NullLoggerFactory()), mockOptions);

            var mockLogger = new Logger<ItemController>(new NullLoggerFactory());
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData["Status"] = "";

            ItemController itemController = new ItemController(context, mockLogger, mockHelper)
            {
                TempData = tempData
            };
            return itemController;
        }

        [Theory]
        [ClassData(typeof(Setup))]
        public async void TestIndexView_Returns_ViewResultAndItemModels(DataDbContext context)
        {
            // Arrange
            using (context)
            {
                ItemController itemController = getController(context);

                // Act
                var result = await itemController.Index("");

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<IEnumerable<Item>>(viewResult.ViewData.Model);
                Assert.True(model.Count() > 0);
            }  
        }

        [Theory]
        [ClassData(typeof(Setup))]
        public void TestCreateView_Get_Returns_ViewResult(DataDbContext context)
        {
            using (context)
            {
                ItemController itemController = getController(context);

                var result = itemController.Create();

                Assert.IsType<ViewResult>(result);
            }
        }

        [Theory]
        [ClassData(typeof(Setup))]
        public void TestCreateView_Post_Handles_InvalidModel(DataDbContext context)
        {
            using (context)
            {
                ItemController itemController = getController(context);
                ItemImageViewModel model = new();

                itemController.ModelState.AddModelError("SessionName", "Required");
                var result = itemController.Create(model, null, null);

                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.True(viewResult.ViewData.ContainsKey("Status"));
            }
        }

        [Theory]
        [ClassData(typeof(Setup))]
        public void TestCreateView_Post_Handles_ValidModel(DataDbContext context)
        {
            using (context)
            {
                ItemController itemController = getController(context);
                ItemImageViewModel model = new();

                Item item = new Item
                {
                    Name = "TestItem 3",
                    Price = 420,
                    Description = "Test Description 3",
                    SellerId = "OwnerID",
                };

                model.Item = item;

                JObject jsonFile = JObject.Parse(File.ReadAllText(@"Static/TestFiles.json"));
                model.ImageFile = (string)jsonFile["Image"];
                model.TextFile = (string)jsonFile["Description"];

                var result = itemController.Create(model, null, null);

                Assert.IsType<RedirectToActionResult>(result);
                Assert.NotNull((from items in context.Items
                                where items.Name == "TestItem 3"
                                select items).FirstOrDefault());
                
            }
        }

        [Theory]
        [ClassData(typeof(Setup))]
        public async void TestEditView_Get_Handles_InvalidModel(DataDbContext context)
        {
            using (context)
            {
                ItemController itemController = getController(context);
                
                var result = await itemController.Edit(125);

                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Theory]
        [ClassData(typeof(Setup))]
        public async void TestEditView_Post_Handles_IdAndModelIdMismatch(DataDbContext context)
        {
            using (context)
            {
                // Arrange
                ItemController itemController = getController(context);
                ItemImageViewModel model = new();
                Item item = (from items in context.Items
                             select items).First();
                item.Description = "This is a new description";
                model.Item = item;

                //Act
                var result = await itemController.Edit(125, model, null, null);

                //Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Theory]
        [ClassData(typeof(Setup))]
        public async void TestEditView_Handles_ValidModel(DataDbContext context)
        {
            using (context)
            {
                // Arrange
                ItemController itemController = getController(context);
                ItemImageViewModel model = new();
                Item item = (from items in context.Items
                             select items).First();
                item.Description = "This is a new description";
                model.Item = item;

                // Act
                var result = await itemController.Edit(item.Id, model, null, null);

                // Assert
                Assert.IsType<RedirectToActionResult>(result);
                Assert.True((from items in context.Items
                             where items.Id == item.Id
                             select items.Description).First() == "This is a new description");
                
            }
        }

        [Theory]
        [ClassData(typeof(Setup))]
        public async void TestRemoveView_Handles_ValidId(DataDbContext context)
        {
            ItemController itemController = getController(context);

            int id = (from items in context.Items
                      select items.Id).First();
            var result = await itemController.Remove(id);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Null((from items in context.Items
                         where items.Id == id
                         select items).FirstOrDefault());
        }
    }
}

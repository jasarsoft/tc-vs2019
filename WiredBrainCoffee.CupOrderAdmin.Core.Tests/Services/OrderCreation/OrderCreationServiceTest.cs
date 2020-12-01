using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WiredBrainCoffee.CupOrderAdmin.Core.Model;
using WiredBrainCoffee.CupOrderAdmin.Core.Services.OrderCreation;

namespace WiredBrainCoffee.CupOrderAdmin.Core.Tests.Services.OrderCreation
{
    [TestClass]
    public class OrderCreationServiceTest
    {
        [TestMethod]
        public async  Task ShouldStoreCreatedOrderInORderCreateionResult()
        {
            var orderCreationService = new OrderCreationService(null, null);

            var numberOfOrderedCups = 1;
            var customer = new Customer() {Id = 99};

            var orderCreationResult = await orderCreationService.CreateOrderAsync(customer, numberOfOrderedCups);

            Assert.AreEqual(OrderCreationResultCode.Success, orderCreationResult.ResultCode);
            Assert.IsNotNull(orderCreationResult.CreatedOrder);
            Assert.AreEqual(customer.Id, orderCreationResult.CreatedOrder.CustomerId);
        }
    }
}

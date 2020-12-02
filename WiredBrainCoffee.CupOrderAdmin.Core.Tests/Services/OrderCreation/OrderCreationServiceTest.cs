using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using WiredBrainCoffee.CupOrderAdmin.Core.DataInterfaces;
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
            var orderRepositoryMock = new Mock<IOrderRepository>();
            orderRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<Order>())).ReturnsAsync((Order x) => x);
            var coffeCupRepositoryMock = new Mock<ICoffeeCupRepository>();

            var orderCreationService = new OrderCreationService(orderRepositoryMock.Object, coffeCupRepositoryMock.Object);

            var numberOfOrderedCups = 1;
            var customer = new Customer() {Id = 99};

            var orderCreationResult = await orderCreationService.CreateOrderAsync(customer, numberOfOrderedCups);

            Assert.AreEqual(OrderCreationResultCode.Success, orderCreationResult.ResultCode);
            Assert.IsNotNull(orderCreationResult.CreatedOrder);
            Assert.AreEqual(customer.Id, orderCreationResult.CreatedOrder.CustomerId);
        }
    }
}

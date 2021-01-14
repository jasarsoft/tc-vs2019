using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using WiredBrainCoffee.CupOrderAdmin.Core.DataInterfaces;
using WiredBrainCoffee.CupOrderAdmin.Core.Model;
using WiredBrainCoffee.CupOrderAdmin.Core.Model.Enums;
using WiredBrainCoffee.CupOrderAdmin.Core.Services.OrderCreation;

namespace WiredBrainCoffee.CupOrderAdmin.Core.Tests.Services.OrderCreation
{
    [TestFixture]
    public class OrderCreationServiceTest
    {
        private OrderCreationService _orderCreationService;
        private int _numberOfCupsInStock;

        [SetUp]
        public void TestInitialize()
        {
            

            var orderRepositoryMock = new Mock<IOrderRepository>();
            orderRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<Order>())).ReturnsAsync((Order x) => x);
            var coffeCupRepositoryMock = new Mock<ICoffeeCupRepository>();

            _numberOfCupsInStock = 10;
            coffeCupRepositoryMock.Setup(x => x.GetCoffeeCupsInStockCountAsync()).ReturnsAsync(_numberOfCupsInStock);

            _orderCreationService = new OrderCreationService(orderRepositoryMock.Object, coffeCupRepositoryMock.Object);
        }

        [Test]
        public async  Task ShouldStoreCreatedOrderInORderCreateionResult()
        {
            var numberOfOrderedCups = 1;
            var customer = new Customer() {Id = 99};

            var orderCreationResult = await _orderCreationService.CreateOrderAsync(customer, numberOfOrderedCups);

            Assert.AreEqual(OrderCreationResultCode.Success, orderCreationResult.ResultCode);
            Assert.IsNotNull(orderCreationResult.CreatedOrder);
            Assert.AreEqual(customer.Id, orderCreationResult.CreatedOrder.CustomerId);
        }

        [Test]
        public async Task ShouldStoreRemainingCupsInStockInOrderCreationResult()
        {
            var numberOfOrderedCups = 3;
            var expectedRemainingCupsInStock = _numberOfCupsInStock - numberOfOrderedCups;
            var customer = new Customer();

            var orderCreationResult = await _orderCreationService.CreateOrderAsync(customer, numberOfOrderedCups);

            Assert.AreEqual(OrderCreationResultCode.Success, orderCreationResult.ResultCode);
            Assert.AreEqual(expectedRemainingCupsInStock, orderCreationResult.RemainingCupsInStock);
        }

        [Test]
        public async void ShouldReturnStockExceededResultIfNotEnoughtCupsInStock()
        {
            var numberOfOrderedCups = _numberOfCupsInStock + 1;
            var customer = new Customer();

            var orderCreationResult = await _orderCreationService.CreateOrderAsync(customer, numberOfOrderedCups);

            Assert.AreEqual(OrderCreationResultCode.StockExceeded, orderCreationResult.ResultCode);
            Assert.AreEqual(_numberOfCupsInStock, orderCreationResult.RemainingCupsInStock);
        }

        [Test]
        public void ShouldThrowExceptionIfNumberOfOrderedCupsIsLessThenOne()
        {
            var numberOfOrderedCups = 0;
            var customer = new Customer();

            var exception = Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _orderCreationService.CreateOrderAsync(customer, numberOfOrderedCups));

            Assert.AreEqual("numberOfOrderedCups", exception.ParamName);
        }
        [Test]
        public void ShouldThrowExceptionIfCustomerIsNull()
        {
            var numberOfOrderedCups = 1;
            Customer customer = null;

            var exception = Assert.ThrowsAsync<ArgumentNullException>(
                () => _orderCreationService.CreateOrderAsync(customer, numberOfOrderedCups));

            Assert.AreEqual("customer", exception.ParamName);
        }

        [TestCase(3,5, CustomerMembership.Basic)]
        [TestCase(0,4, CustomerMembership.Basic)]
        [TestCase(8,5, CustomerMembership.Premium)]
        [TestCase(5,4, CustomerMembership.Premium)]
        public async Task ShouldCalculateCorrectDiscountPercentage(double expectedDiscountInPercent, int numberOfOrderCups, CustomerMembership customerMembership)
        {
            var customer = new Customer {Membership = customerMembership};

            var orderCreationResult = await _orderCreationService.CreateOrderAsync(customer, numberOfOrderCups);

            Assert.AreEqual(OrderCreationResultCode.Success, orderCreationResult.ResultCode);
            Assert.AreEqual(expectedDiscountInPercent, orderCreationResult.CreatedOrder.DiscountInPercent);
        }

        [TestCase(3,5, CustomerMembership.Basic)]
        [TestCase(0,4, CustomerMembership.Basic)]
        [TestCase(0,1, CustomerMembership.Basic)]
        [TestCase(8,5, CustomerMembership.Premium)]
        [TestCase(5,4, CustomerMembership.Premium)]
        [TestCase(5,1, CustomerMembership.Premium)]
        public void ShouldCalculateCorrectDiscountPercentage2(double expectedDiscountInPercent, int numberOfOrderCups, CustomerMembership customerMembership)
        {
            var discountInPercent =
                OrderCreationService.CalculateDiscountPercentage(customerMembership, numberOfOrderCups);
            Assert.AreEqual(expectedDiscountInPercent, discountInPercent);
        }
     }
}

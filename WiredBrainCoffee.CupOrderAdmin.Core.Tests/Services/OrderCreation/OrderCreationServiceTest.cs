using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using WiredBrainCoffee.CupOrderAdmin.Core.DataInterfaces;
using WiredBrainCoffee.CupOrderAdmin.Core.Model;
using WiredBrainCoffee.CupOrderAdmin.Core.Model.Enums;
using WiredBrainCoffee.CupOrderAdmin.Core.Services.OrderCreation;
using Xunit;

namespace WiredBrainCoffee.CupOrderAdmin.Core.Tests.Services.OrderCreation
{
    public class OrderCreationServiceTest
    {
        private OrderCreationService _orderCreationService;
        private int _numberOfCupsInStock;

        public OrderCreationServiceTest()
        {
            var orderRepositoryMock = new Mock<IOrderRepository>();
            orderRepositoryMock.Setup(x => x.SaveAsync(It.IsAny<Order>())).ReturnsAsync((Order x) => x);
            var coffeCupRepositoryMock = new Mock<ICoffeeCupRepository>();

            _numberOfCupsInStock = 10;
            coffeCupRepositoryMock.Setup(x => x.GetCoffeeCupsInStockCountAsync()).ReturnsAsync(_numberOfCupsInStock);
            coffeCupRepositoryMock.Setup(x => x.GetCoffeeCupsInStockAsync(It.IsAny<int>())).ReturnsAsync(
                (int numberOfOrderedCups) => Enumerable.Range(1, numberOfOrderedCups).Select(p => new CoffeeCup()));

            _orderCreationService = new OrderCreationService(orderRepositoryMock.Object, coffeCupRepositoryMock.Object);
        }

        [Fact]
        public async  Task ShouldStoreCreatedOrderInORderCreateionResult()
        {
            var numberOfOrderedCups = 1;
            var customer = new Customer() {Id = 99};

            var orderCreationResult = await _orderCreationService.CreateOrderAsync(customer, numberOfOrderedCups);

            Assert.Equal(OrderCreationResultCode.Success, orderCreationResult.ResultCode);
            Assert.NotNull(orderCreationResult.CreatedOrder);
            Assert.Equal(customer.Id, orderCreationResult.CreatedOrder.CustomerId);
        }

        [Fact]
        public async Task ShouldStoreRemainingCupsInStockInOrderCreationResult()
        {
            var numberOfOrderedCups = 3;
            var expectedRemainingCupsInStock = _numberOfCupsInStock - numberOfOrderedCups;
            var customer = new Customer();

            var orderCreationResult = await _orderCreationService.CreateOrderAsync(customer, numberOfOrderedCups);

            Assert.Equal(OrderCreationResultCode.Success, orderCreationResult.ResultCode);
            Assert.Equal(expectedRemainingCupsInStock, orderCreationResult.RemainingCupsInStock);
        }

        [Fact]
        public async void ShouldReturnStockExceededResultIfNotEnoughtCupsInStock()
        {
            var numberOfOrderedCups = _numberOfCupsInStock + 1;
            var customer = new Customer();

            var orderCreationResult = await _orderCreationService.CreateOrderAsync(customer, numberOfOrderedCups);

            Assert.Equal(OrderCreationResultCode.StockExceeded, orderCreationResult.ResultCode);
            Assert.Equal(_numberOfCupsInStock, orderCreationResult.RemainingCupsInStock);
        }

        [Fact]
        public async void ShouldThrowExceptionIfNumberOfOrderedCupsIsLessThenOne()
        {
            var numberOfOrderedCups = 0;
            var customer = new Customer();

            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _orderCreationService.CreateOrderAsync(customer, numberOfOrderedCups));

            Assert.Equal("numberOfOrderedCups", exception.ParamName);
        }
        [Fact]
        public async void ShouldThrowExceptionIfCustomerIsNull()
        {
            var numberOfOrderedCups = 1;
            Customer customer = null;

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _orderCreationService.CreateOrderAsync(customer, numberOfOrderedCups));

            Assert.Equal("customer", exception.ParamName);
        }

        [Theory]
        [InlineData(3,5, CustomerMembership.Basic)]
        [InlineData(0,4, CustomerMembership.Basic)]
        [InlineData(8,5, CustomerMembership.Premium)]
        [InlineData(5,4, CustomerMembership.Premium)]
        public async Task ShouldCalculateCorrectDiscountPercentage(double expectedDiscountInPercent, int numberOfOrderCups, CustomerMembership customerMembership)
        {
            var customer = new Customer {Membership = customerMembership};

            var orderCreationResult = await _orderCreationService.CreateOrderAsync(customer, numberOfOrderCups);

            Assert.Equal(OrderCreationResultCode.Success, orderCreationResult.ResultCode);
            Assert.Equal(expectedDiscountInPercent, orderCreationResult.CreatedOrder.DiscountInPercent);
        }

        [Theory]
        [InlineData(3,5, CustomerMembership.Basic)]
        [InlineData(0,4, CustomerMembership.Basic)]
        [InlineData(0,1, CustomerMembership.Basic)]
        [InlineData(8,5, CustomerMembership.Premium)]
        [InlineData(5,4, CustomerMembership.Premium)]
        [InlineData(5,1, CustomerMembership.Premium)]
        public void ShouldCalculateCorrectDiscountPercentage2(double expectedDiscountInPercent, int numberOfOrderCups, CustomerMembership customerMembership)
        {
            var discountInPercent =
                OrderCreationService.CalculateDiscountPercentage(customerMembership, numberOfOrderCups);
            Assert.Equal(expectedDiscountInPercent, discountInPercent);
        }

        [Fact]
        public void ShouldThrowArgumentNullExceptionIfOrderRepositoryIsNull()
        {
            var exception =
                Assert.Throws<ArgumentNullException>(
                    () => new OrderCreationService(null, new Mock<ICoffeeCupRepository>().Object));

            Assert.Equal("orderRepository", exception.ParamName);
        }

        [Fact]
        public void ShouldThrowArgumentNullExceptionIfCoffeCupRepositoryIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new OrderCreationService(new Mock<IOrderRepository>().Object, null));

            Assert.Equal("coffeeCupRepository", exception.ParamName);
        }
     }
}

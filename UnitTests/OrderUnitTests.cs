using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using DataAccess;
using LAB4_150348.Controllers;
using DataAccess.Models;
using DataAccess.Repositories;
using Infrastructure.Services;
using LAB4_150348.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using UnitTests.Extensions;

namespace UnitTests
{
    public class OrderUnitTests
    {
        private readonly IFixture _fixture = new Fixture();
        private Mock<IOrderService> _orderServiceMock;
        private Mock<IRepository<Order>> _orderRepositoryMock;
        private Mock<IRepository<Book>> _bookRepositoryMock;

        [SetUp]
        public void Setup()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _orderRepositoryMock = new Mock<IRepository<Order>>();
            _bookRepositoryMock = new Mock<IRepository<Book>>();
        }

        private OrdersController GetOrdersController() => new OrdersController(_orderServiceMock.Object, _orderRepositoryMock.Object, _bookRepositoryMock.Object);

        [Test]
        public async Task ShouldReturnAllOrders()
        {
            // Arrange
            var orders = _fixture
                .Build<Order>()
                .Without(order => order.BookOrders)
                .CreateMany()
                .ToList();

            _orderRepositoryMock
                .Setup(repository => repository.GetAll())
                .ReturnsAsync(orders);

            // Act
            var result = await GetOrdersController().Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(orders, result.Model as ICollection<Order>);
        }

        [Test]
        public async Task ShouldCreateEmptyOrderIfIdIsNotGiven()
        {
            // Arrange

            // Act
            var result = await GetOrdersController().Create(null) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Order>(result.Model);
            Assert.AreEqual(((Order) result.Model).TotalPrice, 0);
            Assert.AreEqual(((Order) result.Model).IsFinished, false);
            Assert.AreEqual(((Order) result.Model).TotalPrice, 0);
            DateTimeAssert.AreEqual(((Order)result.Model).Created, DateTime.Now, TimeSpan.FromSeconds(1));
            CollectionAssert.IsEmpty(((Order)result.Model).BookOrders);
            _orderRepositoryMock.Verify(repository => repository.Create(It.IsAny<Order>()), Times.Once);
        }

        [Test]
        public async Task ShouldReturnsOrderIfIdIsGiven()
        {
            // Arrange
            var order = _fixture
                .Build<Order>()
                .Without(order1 => order1.Id)
                .Without(order1 => order1.BookOrders)
                .Create();

            _orderRepositoryMock
                .Setup(repository => repository.Get(It.Is<int>(i => i == order.Id)))
                .ReturnsAsync(order);

            // Act
            var result = await GetOrdersController().Details(order.Id) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Model, order);
        }

        [Test]
        public async Task ShouldReturnsNotFoundIfOrderDoesNotExists()
        {
            // Arrange
            _orderRepositoryMock
                .Setup(repository => repository.Get(It.IsAny<int>()))
                .ReturnsAsync((Order)null);

            // Act
            var result = await GetOrdersController().Details(_fixture.Create<int>());

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task SubmitShouldReturnsNotFoundIfOrderDoesNotExists()
        {
            // Arrange
            _orderRepositoryMock
                .Setup(repository => repository.Get(It.IsAny<int>()))
                .ReturnsAsync((Order) null);

            // Act
            var result = await GetOrdersController().Submit(_fixture.Create<int>());

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [TestCase(40)]
        [TestCase(1000)]
        public async Task SubmitShouldFinishOrderIfExistsAndPriceIsGreaterThan40(decimal price)
        {
            // Arrange
            var book = _fixture
                .Build<Book>()
                .Without(order1 => order1.Id)
                .Without(order1 => order1.BookOrders)
                .With(order1 => order1.AvailableAmount)
                .With(book1 => book1.Price, price)
                .Create();

            var bookOrders = _fixture
                .Build<BookOrder>()
                .With(bookOrder => bookOrder.Book, book)
                .With(bookOrder => bookOrder.BookAmount, book.AvailableAmount)
                .Without(bookOrder => bookOrder.Order)
                .CreateMany(1)
                .ToList();

            var order = _fixture
                .Build<Order>()
                .With(order1 => order1.BookOrders, bookOrders)
                .With(order1 => order1.TotalPrice, book.Price * book.AvailableAmount)
                .Create();

            _orderRepositoryMock
                .Setup(repository => repository.Get(It.Is<int>(i => i == order.Id)))
                .ReturnsAsync(order);

            // Act
            var result = await GetOrdersController().Submit(order.Id);

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.IsTrue(order.IsFinished);
            Assert.AreEqual(book.AvailableAmount, 0);
            _orderRepositoryMock.Verify(repository => repository.Update(It.Is<Order>(expected => expected == order)), Times.Once);
        }

        [TestCase(10, 20)]
        [TestCase(100, 100)]
        [TestCase(1, 2)]
        [TestCase(2, 1)]
        public async Task ShouldUpdateOrderTotalPrice(decimal oldPrice, decimal newPrice)
        {
            // Arrange
            var order = _fixture
                .Build<Order>()
                .Without(order1 => order1.Id)
                .Without(order1 => order1.BookOrders)
                .With(order1 => order1.TotalPrice, oldPrice)
                .Create();

            _orderRepositoryMock
                .Setup(repository => repository.Get(order.Id))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(repository => repository.Update(It.IsAny<Order>()))
                .Callback<Order>(orderCallback => order.TotalPrice = orderCallback.TotalPrice);

            // Act
            var result = await GetOrdersController().Edit(order.Id, new Order {TotalPrice = newPrice}) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(order.TotalPrice, newPrice);
        }

        [Test]
        public async Task DeleteShouldCallRemoveToRepository()
        {
            // Arrange
            var id = _fixture.Create<int>();
            var wasRemoveCalled = false;

            _orderRepositoryMock
                .Setup(repository => repository.Remove(It.Is<int>(i => i == id)))
                .Callback(() => wasRemoveCalled = true);

            // Act
            var result = await GetOrdersController().DeleteConfirmed(id) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(wasRemoveCalled);
        }

        [TestCase(0)]
        [TestCase(10)]
        [TestCase(39)]
        [TestCase(-10)]
        public async Task ShouldReturnsBadRequestIfTotalPriceOfOrderIsLowerThan40(int amount)
        {
            // Arrange
            var order = _fixture
                .Build<Order>()
                .Without(order1 => order1.Id)
                .Without(order1 => order1.BookOrders)
                .With(order1 => order1.TotalPrice, amount)
                .Create();

            _orderRepositoryMock
                .Setup(repository => repository.Get(order.Id))
                .ReturnsAsync(order);

            // Act
            var result = await GetOrdersController().Submit(order.Id) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Value, "Order price must be greater than 40.");
        }

        [TestCase(0, 10)]
        [TestCase(0, 1)]
        [TestCase(10, 11)]
        [TestCase(39, 40)]
        [TestCase(1, 2)]
        public async Task ShouldReturnsBadRequestIfOrdersMoreBooksThanAreAvailable(int actualBookAmount, int requestedBookAmount)
        {
            // Arrange
            var order = _fixture
                .Build<Order>()
                .Without(order1 => order1.Id)
                .Without(order1 => order1.BookOrders)
                .With(order1 => order1.TotalPrice, 0)
                .With(order1 => order1.IsFinished, false)
                .Create();

            var book = _fixture
                .Build<Book>()
                .Without(order1 => order1.Id)
                .Without(order1 => order1.BookOrders)
                .With(order1 => order1.AvailableAmount, actualBookAmount)
                .Create();

            _orderRepositoryMock
                .Setup(repository => repository.Get(It.Is<int>(i => i == order.Id)))
                .ReturnsAsync(order);

            _bookRepositoryMock
                .Setup(repository => repository.Get(It.Is<int>(i => i == book.Id)))
                .ReturnsAsync(book);

            // Act
            var result = await GetOrdersController().AddBookToOrder(new BookOrderViewModel
            {
                OrderId = order.Id,
                BookAmount = requestedBookAmount,
                BookId = book.Id
            }) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Value, "Cannot order more books than are available.");
        }

        [Test]
        public async Task ShouldReturnsBadRequestIfOrderedBookDoesNotExists()
        {
            // Arrange
            var order = _fixture
                .Build<Order>()
                .Without(order1 => order1.Id)
                .Without(order1 => order1.BookOrders)
                .Create();
            var bookId = _fixture.Create<int>();

            _orderRepositoryMock
                .Setup(repository => repository.Get(It.Is<int>(i => i == order.Id)))
                .ReturnsAsync(order);

            // Act
            var result = await GetOrdersController().AddBookToOrder(new BookOrderViewModel
            {
                OrderId = order.Id,
                BookAmount = _fixture.Create<int>(),
                BookId = bookId
            }) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Value, $"Book with id {bookId} does not exists.");
        }

        [Test]
        public async Task ShouldReturnsBadRequestIfOrderDoesNotExists()
        {
            // Arrange
            var orderId = _fixture.Create<int>();
            var book = _fixture
                .Build<Book>()
                .Without(book1 => book1.Id)
                .Without(book1 => book1.BookOrders)
                .With(book1 => book1.AvailableAmount)
                .Create();

            _bookRepositoryMock
                .Setup(repository => repository.Get(It.Is<int>(i => i == book.Id)))
                .ReturnsAsync(book);

            // Act
            var result = await GetOrdersController().AddBookToOrder(new BookOrderViewModel
            {
                OrderId = orderId,
                BookAmount = _fixture.Create<int>(),
                BookId = book.Id
            }) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Value, $"Order with id {orderId} does not exists.");
        }
    }
}
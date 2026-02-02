using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.Enums;
using OnlineBookManagementSystem.Core.Domain.ValueObjects;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Infrastructure.Data.Repositories;
using OnlineBookManagementSystem.Infrastructure.Services.Domain.Orders;
using OnlineBookManagementSystem.Presentation.ViewModels.Cart;

namespace OnlineBookManagementSystem.Tests.Commands;

public class OrderCommandTests : IDisposable
{
    private readonly BookManagementContext _context;
    private readonly RefactoredOrderCommandService _orderService;
    private readonly Mock<ILogger<RefactoredOrderCommandService>> _loggerMock;
    private readonly Mock<IActivityLogger> _activityLoggerMock;
    private readonly IBookRepository _bookRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderCommandTests()
    {
        var options = new DbContextOptionsBuilder<BookManagementContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookManagementContext(options);
        _loggerMock = new Mock<ILogger<RefactoredOrderCommandService>>();
        _activityLoggerMock = new Mock<IActivityLogger>();
        
        _bookRepository = new BookRepository(_context);
        _orderRepository = new OrderRepository(_context);
        _unitOfWork = new UnitOfWork(_context);

        _orderService = new RefactoredOrderCommandService(
            _orderRepository,
            _bookRepository,
            _unitOfWork,
            _loggerMock.Object,
            _activityLoggerMock.Object
        );
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidItems_CreatesOrderAndDeductsStock()
    {
        // Arrange
        var book1 = CreateTestBook("Book 1", 10.00m, 5);
        var book2 = CreateTestBook("Book 2", 15.00m, 3);
        
        _context.Books.AddRange(book1, book2);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            UserId = 1,
            Email = "test@example.com",
            Phone = "123-456-7890",
            PaymentMethod = "Credit Card",
            FullName = "John Doe",
            Address = "123 Main St",
            City = "Anytown",
            State = "CA",
            ZipCode = "12345",
            Items = new List<CartItemRequestViewModel>
            {
                new() { BookId = book1.Id, Quantity = 2, Price = 10.00m },
                new() { BookId = book2.Id, Quantity = 1, Price = 15.00m }
            }
        };

        // Act
        var result = await _orderService.CreateOrderAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.OrderId.Should().BeGreaterThan(0);
        result.Message.Should().Contain("successfully");

        // Verify stock deduction
        var updatedBook1 = await _bookRepository.GetByIdAsync(book1.Id);
        var updatedBook2 = await _bookRepository.GetByIdAsync(book2.Id);
        
        updatedBook1!.StockQuantity.Should().Be(3); // 5 - 2 = 3
        updatedBook2!.StockQuantity.Should().Be(2); // 3 - 1 = 2

        // Verify order creation
        var order = await _orderRepository.GetByIdAsync(result.OrderId);
        order.Should().NotBeNull();
        order!.TotalAmount.Amount.Should().Be(35.00m); // (10*2) + (15*1) = 35
        order.Status.Should().Be(OrderStatus.Pending);
        order.PaymentStatus.Should().Be(PaymentStatus.Pending);
        order.OrderDetails.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateOrderAsync_WithInsufficientStock_FailsAndDoesNotDeductStock()
    {
        // Arrange
        var book = CreateTestBook("Low Stock Book", 20.00m, 2);
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            UserId = 1,
            Email = "test@example.com",
            Items = new List<CartItemRequestViewModel>
            {
                new() { BookId = book.Id, Quantity = 5, Price = 20.00m } // Requesting more than available
            }
        };

        // Act
        var result = await _orderService.CreateOrderAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.OrderId.Should().Be(0);
        result.Message.Should().Contain("Insufficient stock");

        // Verify stock was NOT deducted
        var unchangedBook = await _bookRepository.GetByIdAsync(book.Id);
        unchangedBook!.StockQuantity.Should().Be(2); // Original stock unchanged
    }

    [Fact]
    public async Task CreateOrderAsync_WithNonExistentBook_FailsGracefully()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            UserId = 1,
            Email = "test@example.com",
            Items = new List<CartItemRequestViewModel>
            {
                new() { BookId = 999, Quantity = 1, Price = 10.00m } // Non-existent book
            }
        };

        // Act
        var result = await _orderService.CreateOrderAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.OrderId.Should().Be(0);
        result.Message.Should().Contain("Book with ID 999 not found");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_FromPendingToDelivered_UpdatesPaymentStatusToPaid()
    {
        // Arrange
        var order = CreateTestOrder(OrderStatus.Pending, PaymentStatus.Pending);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Delivered, 1);

        // Assert
        result.Should().BeTrue();

        var updatedOrder = await _orderRepository.GetByIdAsync(order.Id);
        updatedOrder!.Status.Should().Be(OrderStatus.Delivered);
        updatedOrder.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ToCancelled_RestoresStockAndSetsRefunded()
    {
        // Arrange
        var book = CreateTestBook("Test Book", 25.00m, 5);
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var order = CreateTestOrder(OrderStatus.Processing, PaymentStatus.Pending);
        var orderDetail = new OrderDetail
        {
            BookId = book.Id,
            Quantity = 2,
            UnitPrice = new Money(25.00m),
            Subtotal = new Money(50.00m)
        };
        order.OrderDetails.Add(orderDetail);
        
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Simulate stock deduction (as would happen during order creation)
        book.StockQuantity = 3; // 5 - 2 = 3
        await _context.SaveChangesAsync();

        // Act
        var result = await _orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Cancelled, 1);

        // Assert
        result.Should().BeTrue();

        var updatedOrder = await _orderRepository.GetByIdAsync(order.Id);
        updatedOrder!.Status.Should().Be(OrderStatus.Cancelled);
        updatedOrder.PaymentStatus.Should().Be(PaymentStatus.Refunded);

        // Verify stock restoration
        var restoredBook = await _bookRepository.GetByIdAsync(book.Id);
        restoredBook!.StockQuantity.Should().Be(5); // 3 + 2 = 5 (restored)
    }

    [Fact]
    public async Task CancelOrderAsync_WithPendingOrder_CancelsSuccessfully()
    {
        // Arrange
        var book = CreateTestBook("Cancellable Book", 30.00m, 10);
        _context.Books.Add(book);
        
        var order = CreateTestOrder(OrderStatus.Pending, PaymentStatus.Pending);
        var orderDetail = new OrderDetail
        {
            BookId = book.Id,
            Quantity = 3,
            UnitPrice = new Money(30.00m),
            Subtotal = new Money(90.00m)
        };
        order.OrderDetails.Add(orderDetail);
        
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _orderService.CancelOrderAsync(order.Id, 1);

        // Assert
        result.Should().BeTrue();

        var cancelledOrder = await _orderRepository.GetByIdAsync(order.Id);
        cancelledOrder!.Status.Should().Be(OrderStatus.Cancelled);
        cancelledOrder.PaymentStatus.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public async Task CancelOrderAsync_WithDeliveredOrder_FailsToCancel()
    {
        // Arrange
        var order = CreateTestOrder(OrderStatus.Delivered, PaymentStatus.Paid);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _orderService.CancelOrderAsync(order.Id, 1);

        // Assert
        result.Should().BeFalse();

        // Verify order status unchanged
        var unchangedOrder = await _orderRepository.GetByIdAsync(order.Id);
        unchangedOrder!.Status.Should().Be(OrderStatus.Delivered);
        unchangedOrder.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    private Book CreateTestBook(string title, decimal price, int stock)
    {
        return new Book
        {
            Title = title,
            Author = "Test Author",
            Price = new Money(price),
            StockQuantity = stock,
            CategoryId = 1,
            CreatedAt = DateTime.UtcNow
        };
    }

    private Order CreateTestOrder(OrderStatus status, PaymentStatus paymentStatus)
    {
        return new Order
        {
            UserId = 1,
            TotalAmount = new Money(100.00m),
            Status = status,
            PaymentStatus = paymentStatus,
            OrderDate = DateTime.UtcNow,
            PaymentMethod = "Test Payment",
            ShippingAddress = new Address("Test User", "123 Test St", "Test City", "TS", "12345", "USA")
        };
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
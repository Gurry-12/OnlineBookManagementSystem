using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure;
using OnlineBookManagementSystem.Core.Application.Interfaces.Repositories;
using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.ValueObjects;
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Infrastructure.Data.Repositories;
using OnlineBookManagementSystem.Infrastructure.Services.Domain.Cart;

namespace OnlineBookManagementSystem.Tests.Commands;

public class CartCommandTests : IDisposable
{
    private readonly BookManagementContext _context;
    private readonly RefactoredCartService _cartService;
    private readonly Mock<ILogger<RefactoredCartService>> _loggerMock;
    private readonly Mock<IActivityLogger> _activityLoggerMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly IBookRepository _bookRepository;
    private readonly ICartRepository _cartRepository;

    public CartCommandTests()
    {
        var options = new DbContextOptionsBuilder<BookManagementContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookManagementContext(options);
        _loggerMock = new Mock<ILogger<RefactoredCartService>>();
        _activityLoggerMock = new Mock<IActivityLogger>();
        _cacheMock = new Mock<IMemoryCache>();
        
        _bookRepository = new BookRepository(_context);
        _cartRepository = new CartRepository(_context);

        _cartService = new RefactoredCartService(
            _cartRepository,
            _bookRepository,
            _cacheMock.Object,
            _loggerMock.Object,
            _activityLoggerMock.Object
        );
    }

    [Fact]
    public async Task AddOrUpdateCartAsync_WithValidBook_AddsToCart()
    {
        // Arrange
        var book = CreateTestBook("Available Book", 15.00m, 10);
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        const int userId = 1;
        const int quantity = 2;

        // Act
        var result = await _cartService.AddOrUpdateCartAsync(userId, book.Id, quantity);

        // Assert
        result.Should().BeTrue();

        var cartItem = await _cartRepository.GetCartItemAsync(userId, book.Id);
        cartItem.Should().NotBeNull();
        cartItem!.Quantity.Should().Be(quantity);
        cartItem.BookId.Should().Be(book.Id);
        cartItem.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task AddOrUpdateCartAsync_WithExistingItem_UpdatesQuantity()
    {
        // Arrange
        var book = CreateTestBook("Existing Book", 20.00m, 15);
        _context.Books.Add(book);
        
        var existingCartItem = new ShoppingCart(1, book.Id, 3);
        _context.ShoppingCarts.Add(existingCartItem);
        await _context.SaveChangesAsync();

        // Act - Add 2 more to existing quantity of 3
        var result = await _cartService.AddOrUpdateCartAsync(1, book.Id, 2);

        // Assert
        result.Should().BeTrue();

        var updatedCartItem = await _cartRepository.GetCartItemAsync(1, book.Id);
        updatedCartItem!.Quantity.Should().Be(5); // 3 + 2 = 5
    }

    [Fact]
    public async Task AddOrUpdateCartAsync_WithInsufficientStock_ReturnsFalse()
    {
        // Arrange
        var book = CreateTestBook("Low Stock Book", 25.00m, 2);
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Act - Try to add more than available stock
        var result = await _cartService.AddOrUpdateCartAsync(1, book.Id, 5);

        // Assert
        result.Should().BeFalse();

        var cartItem = await _cartRepository.GetCartItemAsync(1, book.Id);
        cartItem.Should().BeNull(); // No cart item should be created
    }

    [Fact]
    public async Task AddOrUpdateCartAsync_WithDeletedBook_ReturnsFalse()
    {
        // Arrange
        var book = CreateTestBook("Deleted Book", 30.00m, 5);
        book.MarkAsDeleted(); // Soft delete the book
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.AddOrUpdateCartAsync(1, book.Id, 1);

        // Assert
        result.Should().BeFalse();

        var cartItem = await _cartRepository.GetCartItemAsync(1, book.Id);
        cartItem.Should().BeNull();
    }

    [Fact]
    public async Task AddOrUpdateCartAsync_WithNonExistentBook_ReturnsFalse()
    {
        // Arrange - No book added to context

        // Act
        var result = await _cartService.AddOrUpdateCartAsync(1, 999, 1);

        // Assert
        result.Should().BeFalse();

        var cartItem = await _cartRepository.GetCartItemAsync(1, 999);
        cartItem.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCartQuantityAsync_WithValidQuantity_UpdatesSuccessfully()
    {
        // Arrange
        var book = CreateTestBook("Update Book", 12.00m, 8);
        _context.Books.Add(book);
        
        var cartItem = new ShoppingCart(1, book.Id, 2);
        _context.ShoppingCarts.Add(cartItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.UpdateCartQuantityAsync(1, book.Id, 4);

        // Assert
        result.Should().BeTrue();

        var updatedItem = await _cartRepository.GetCartItemAsync(1, book.Id);
        updatedItem!.Quantity.Should().Be(4);
    }

    [Fact]
    public async Task UpdateCartQuantityAsync_WithQuantityExceedingStock_ReturnsFalse()
    {
        // Arrange
        var book = CreateTestBook("Limited Stock", 18.00m, 3);
        _context.Books.Add(book);
        
        var cartItem = new ShoppingCart(1, book.Id, 1);
        _context.ShoppingCarts.Add(cartItem);
        await _context.SaveChangesAsync();

        // Act - Try to update to more than available stock
        var result = await _cartService.UpdateCartQuantityAsync(1, book.Id, 5);

        // Assert
        result.Should().BeFalse();

        var unchangedItem = await _cartRepository.GetCartItemAsync(1, book.Id);
        unchangedItem!.Quantity.Should().Be(1); // Original quantity unchanged
    }

    [Fact]
    public async Task UpdateCartQuantityAsync_WithZeroQuantity_RemovesItem()
    {
        // Arrange
        var book = CreateTestBook("Remove Book", 22.00m, 5);
        _context.Books.Add(book);
        
        var cartItem = new ShoppingCart(1, book.Id, 3);
        _context.ShoppingCarts.Add(cartItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.UpdateCartQuantityAsync(1, book.Id, 0);

        // Assert
        result.Should().BeTrue();

        var removedItem = await _cartRepository.GetCartItemAsync(1, book.Id);
        removedItem.Should().BeNull(); // Item should be removed
    }

    [Fact]
    public async Task RemoveCartItemAsync_WithExistingItem_RemovesSuccessfully()
    {
        // Arrange
        var book = CreateTestBook("Remove Test Book", 14.00m, 6);
        _context.Books.Add(book);
        
        var cartItem = new ShoppingCart(1, book.Id, 2);
        _context.ShoppingCarts.Add(cartItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.RemoveCartItemAsync(1, book.Id);

        // Assert
        result.Should().BeTrue();

        var removedItem = await _cartRepository.GetCartItemAsync(1, book.Id);
        removedItem.Should().BeNull();
    }

    [Fact]
    public async Task RemoveCartItemAsync_WithNonExistentItem_ReturnsFalse()
    {
        // Arrange - No cart item exists

        // Act
        var result = await _cartService.RemoveCartItemAsync(1, 999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ClearCartAsync_WithMultipleItems_RemovesAllItems()
    {
        // Arrange
        var book1 = CreateTestBook("Book 1", 10.00m, 5);
        var book2 = CreateTestBook("Book 2", 15.00m, 3);
        _context.Books.AddRange(book1, book2);
        
        var cartItem1 = new ShoppingCart(1, book1.Id, 2);
        var cartItem2 = new ShoppingCart(1, book2.Id, 1);
        _context.ShoppingCarts.AddRange(cartItem1, cartItem2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _cartService.ClearCartAsync(1);

        // Assert
        result.Should().BeTrue();

        var remainingItems = await _cartRepository.GetUserCartItemsAsync(1);
        remainingItems.Should().BeEmpty();
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

    public void Dispose()
    {
        _context.Dispose();
    }
}
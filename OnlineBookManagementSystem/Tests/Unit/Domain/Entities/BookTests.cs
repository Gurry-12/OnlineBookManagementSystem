using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Core.Domain.ValueObjects;
using Xunit;

namespace OnlineBookManagementSystem.Tests.Unit.Domain.Entities
{
    /// <summary>
    /// Unit tests for Book entity to ensure domain logic is correct.
    /// Tests business rules, validation, and behavior.
    /// </summary>
    public class BookTests
    {
        [Fact]
        public void Book_Constructor_ShouldSetDefaultValues()
        {
            // Arrange & Act
            var book = new Book();

            // Assert
            Assert.False(book.IsDeleted);
            Assert.True(book.CreatedAt <= DateTime.UtcNow);
            Assert.True(book.UpdatedAt <= DateTime.UtcNow);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Book_SetTitle_WithInvalidValue_ShouldThrowArgumentException(string invalidTitle)
        {
            // Arrange
            var book = new Book();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => book.Title = invalidTitle);
        }

        [Fact]
        public void Book_SetTitle_WithValidValue_ShouldSetTitleAndTrim()
        {
            // Arrange
            var book = new Book();
            var title = "  The Great Gatsby  ";

            // Act
            book.Title = title;

            // Assert
            Assert.Equal("The Great Gatsby", book.Title);
        }

        [Fact]
        public void Book_SetTitle_WithTooLongValue_ShouldThrowArgumentException()
        {
            // Arrange
            var book = new Book();
            var longTitle = new string('A', 201); // 201 characters

            // Act & Assert
            Assert.Throws<ArgumentException>(() => book.Title = longTitle);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Book_SetAuthor_WithInvalidValue_ShouldThrowArgumentException(string invalidAuthor)
        {
            // Arrange
            var book = new Book();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => book.Author = invalidAuthor);
        }

        [Fact]
        public void Book_SetAuthor_WithValidValue_ShouldSetAuthorAndTrim()
        {
            // Arrange
            var book = new Book();
            var author = "  F. Scott Fitzgerald  ";

            // Act
            book.Author = author;

            // Assert
            Assert.Equal("F. Scott Fitzgerald", book.Author);
        }

        [Fact]
        public void Book_SetPrice_WithNullValue_ShouldThrowArgumentNullException()
        {
            // Arrange
            var book = new Book();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => book.Price = null);
        }

        [Fact]
        public void Book_SetPrice_WithValidValue_ShouldSetPrice()
        {
            // Arrange
            var book = new Book();
            var price = new Money(19.99m);

            // Act
            book.Price = price;

            // Assert
            Assert.Equal(price, book.Price);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Book_SetStockQuantity_WithNegativeValue_ShouldThrowArgumentException(int negativeStock)
        {
            // Arrange
            var book = new Book();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => book.StockQuantity = negativeStock);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(100)]
        public void Book_SetStockQuantity_WithValidValue_ShouldSetStock(int validStock)
        {
            // Arrange
            var book = new Book();

            // Act
            book.StockQuantity = validStock;

            // Assert
            Assert.Equal(validStock, book.StockQuantity);
        }

        [Theory]
        [InlineData(0, 5, true)]
        [InlineData(3, 5, true)]
        [InlineData(5, 5, false)]
        [InlineData(10, 5, false)]
        public void Book_IsLowStock_ShouldReturnCorrectValue(int stockQuantity, int threshold, bool expectedResult)
        {
            // Arrange
            var book = new Book
            {
                StockQuantity = stockQuantity,
                LowStockThreshold = threshold
            };

            // Act
            var result = book.IsLowStock;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(10, true)]
        public void Book_IsInStock_ShouldReturnCorrectValue(int stockQuantity, bool expectedResult)
        {
            // Arrange
            var book = new Book
            {
                StockQuantity = stockQuantity
            };

            // Act
            var result = book.IsInStock;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Book_ReduceStock_WithSufficientStock_ShouldReduceQuantity()
        {
            // Arrange
            var book = new Book
            {
                StockQuantity = 10
            };

            // Act
            book.ReduceStock(3);

            // Assert
            Assert.Equal(7, book.StockQuantity);
        }

        [Fact]
        public void Book_ReduceStock_WithInsufficientStock_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var book = new Book
            {
                StockQuantity = 2
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => book.ReduceStock(5));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Book_ReduceStock_WithInvalidQuantity_ShouldThrowArgumentException(int invalidQuantity)
        {
            // Arrange
            var book = new Book
            {
                StockQuantity = 10
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => book.ReduceStock(invalidQuantity));
        }

        [Fact]
        public void Book_RestoreStock_WithValidQuantity_ShouldIncreaseStock()
        {
            // Arrange
            var book = new Book
            {
                StockQuantity = 5
            };

            // Act
            book.RestoreStock(3);

            // Assert
            Assert.Equal(8, book.StockQuantity);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Book_RestoreStock_WithInvalidQuantity_ShouldThrowArgumentException(int invalidQuantity)
        {
            // Arrange
            var book = new Book
            {
                StockQuantity = 5
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => book.RestoreStock(invalidQuantity));
        }

        [Fact]
        public void Book_UpdateTimestamp_ShouldUpdateUpdatedAt()
        {
            // Arrange
            var book = new Book();
            var originalTimestamp = book.UpdatedAt;
            
            // Wait a small amount to ensure timestamp difference
            Thread.Sleep(1);

            // Act
            book.UpdateTimestamp();

            // Assert
            Assert.True(book.UpdatedAt > originalTimestamp);
        }

        [Fact]
        public void Book_MarkAsDeleted_ShouldSetIsDeletedAndUpdateTimestamp()
        {
            // Arrange
            var book = new Book();
            var originalTimestamp = book.UpdatedAt;
            
            // Wait a small amount to ensure timestamp difference
            Thread.Sleep(1);

            // Act
            book.MarkAsDeleted();

            // Assert
            Assert.True(book.IsDeleted);
            Assert.True(book.UpdatedAt > originalTimestamp);
        }
    }
}
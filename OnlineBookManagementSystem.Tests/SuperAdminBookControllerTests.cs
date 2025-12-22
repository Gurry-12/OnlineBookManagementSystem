using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using OnlineBookManagementSystem.Controllers.SuperAdmin;
using OnlineBookManagementSystem.Services.SuperAdmin;
using OnlineBookManagementSystem.Models.DTOs;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace OnlineBookManagementSystem.Tests.Controllers.SuperAdmin
{
    public class BookControllerTests
    {
        private readonly Mock<ISuperAdminBookService> _mockService;
        private readonly BookController _controller;

        public BookControllerTests()
        {
            _mockService = new Mock<ISuperAdminBookService>();
            _controller = new BookController(_mockService.Object);

            // Mock User
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "SuperAdmin")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task Create_ReturnsRedirect_WhenServiceReturnsBook()
        {
            // Arrange
            var model = new Models.ViewModel.BookFormViewModel
            {
                Book = new Models.Book { Title = "Test Book" }
            };
            var resultDto = new BookDto { Id = 1, Title = "Test Book" };

            _mockService.Setup(s => s.CreateBookAsync(It.IsAny<CreateBookDto>(), null))
                .ReturnsAsync(resultDto);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("BookList", redirectResult.ActionName);
            _mockService.Verify(s => s.CreateBookAsync(It.IsAny<CreateBookDto>(), null), Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsJsonSuccess_WhenServiceReturnsTrue()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteBookAsync(1, 1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            // Verify call
            _mockService.Verify(s => s.DeleteBookAsync(1, 1), Times.Once);
        }
    }
}

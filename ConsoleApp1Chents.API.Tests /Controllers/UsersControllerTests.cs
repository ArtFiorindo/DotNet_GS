using Chents_GS;
using Chents_GS.Controllers;
using Chents.API.Controllers;
using Chents.Models;
using Chents.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Chents.API.Tests.Controllers;

public class UsersControllerTests
{
    private readonly UsersController _controller;
    private readonly ApplicationDbContext _context;

    public UsersControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        
        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
        
        _controller = new UsersController(_context);
    }

    [Fact]
    public async Task GetUsers_ReturnsAllUsers()
    {
        // Arrange
        _context.Users.Add(new User { Id = Guid.NewGuid(), Name = "Test User", Email = "test@example.com" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var users = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
        Assert.Single(users);
    }

    [Fact]
    public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Act
        var result = await _controller.GetUser(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PostUser_CreatesNewUser()
    {
        // Arrange
        var newUser = new User { Name = "New User", Email = "new@example.com" };

        // Act
        var result = await _controller.PostUser(newUser);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var user = Assert.IsType<User>(createdAtActionResult.Value);
        Assert.Equal("New User", user.Name);
        Assert.Equal("new@example.com", user.Email);
    }
}
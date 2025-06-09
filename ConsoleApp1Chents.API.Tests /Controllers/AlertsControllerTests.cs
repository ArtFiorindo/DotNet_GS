using Chents_GS;
using Chents.API.Controllers;
using Chents.API.Services;
using Chents.Models;
using Chents.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Chents.API.Tests.Controllers;

public class AlertsControllerTests
{
    private readonly AlertsController _controller;
    private readonly ApplicationDbContext _context;
    private readonly Mock<RabbitMQService> _rabbitMQMock;
    private readonly Mock<FloodPredictionService> _predictionMock;

    public AlertsControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        
        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
        
        _rabbitMQMock = new Mock<RabbitMQService>();
        _predictionMock = new Mock<FloodPredictionService>();
        _predictionMock.Setup(p => p.PredictSeverity(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(AlertSeverity.Medium);
        
        _controller = new AlertsController(_context, _rabbitMQMock.Object, _predictionMock.Object);
    }

    [Fact]
    public async Task GetAlerts_ReturnsAlertsFilteredByCity()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Name = "Test User", Email = "test@example.com" };
        _context.Users.Add(user);
        
        _context.Alerts.Add(new Alert { 
            Id = Guid.NewGuid(), 
            Message = "Test Alert", 
            City = "São Paulo",
            Latitude = -23.5505,
            Longitude = -46.6333,
            UserId = user.Id
        });
        
        _context.Alerts.Add(new Alert { 
            Id = Guid.NewGuid(), 
            Message = "Another Alert", 
            City = "Rio de Janeiro",
            Latitude = -22.9068,
            Longitude = -43.1729,
            UserId = user.Id
        });
        
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAlerts(new AlertQuery { City = "São Paulo" });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var alerts = Assert.IsAssignableFrom<IEnumerable<Alert>>(okResult.Value);
        Assert.Single(alerts);
        Assert.All(alerts, a => Assert.Equal("São Paulo", a.City));
    }

    [Fact]
    public async Task PostAlert_CreatesNewAlert()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Name = "Test User", Email = "test@example.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        var newAlert = new Alert { 
            Message = "New Alert", 
            City = "São Paulo",
            Latitude = -23.5505,
            Longitude = -46.6333,
            UserId = user.Id
        };

        // Act
        var result = await _controller.PostAlert(newAlert);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var alert = Assert.IsType<Alert>(createdAtActionResult.Value);
        Assert.Equal("New Alert", alert.Message);
        Assert.Equal("São Paulo", alert.City);
        _rabbitMQMock.Verify(r => r.PublishAlert(It.IsAny<Alert>()), Times.Once);
    }
}
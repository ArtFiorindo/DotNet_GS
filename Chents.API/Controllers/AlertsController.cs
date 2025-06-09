using Chents.API.Services;
using Chents.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using Chents_GS;
using Chents.Models.Models;

namespace Chents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly RabbitMQService _rabbitMQService;
    private readonly FloodPredictionService _predictionService;
    private readonly GeometryFactory _geometryFactory;

    public AlertsController(
        ApplicationDbContext context,
        RabbitMQService rabbitMQService,
        FloodPredictionService predictionService)
    {
        _context = context;
        _rabbitMQService = rabbitMQService;
        _predictionService = predictionService;
        _geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326); // WGS84
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Alert>>> GetAlerts([FromQuery] AlertQuery query)
    {
        IQueryable<Alert> alertsQuery = _context.Alerts.Include(a => a.User);

        if (query.Days.HasValue)
        {
            var dateFilter = DateTime.UtcNow.AddDays(-query.Days.Value);
            alertsQuery = alertsQuery.Where(a => a.CreatedAt >= dateFilter);
        }

        if (!string.IsNullOrEmpty(query.City))
        {
            alertsQuery = alertsQuery.Where(a => a.City == query.City);
        }

        if (query.Latitude.HasValue && query.Longitude.HasValue && query.RadiusKm.HasValue)
        {
            var center = _geometryFactory.CreatePoint(new Coordinate(
                query.Longitude.Value, 
                query.Latitude.Value));
            
            alertsQuery = alertsQuery
                .AsEnumerable()
                .Where(a => 
                {
                    var alertPoint = _geometryFactory.CreatePoint(
                        new Coordinate(a.Longitude, a.Latitude));
                    return alertPoint.Distance(center) <= query.RadiusKm * 1000; // Convert km to meters
                })
                .AsQueryable();
        }

        if (query.Limit.HasValue)
        {
            alertsQuery = alertsQuery
                .OrderByDescending(a => a.CreatedAt)
                .Take(query.Limit.Value);
        }

        var alerts = await alertsQuery.ToListAsync();
        return Ok(AddHateoasLinks(alerts));
    }

    // ... (keep all other action methods the same as before)

    private dynamic AddHateoasLinks(Alert alert)
    {
        return new
        {
            alert.Id,
            alert.Message,
            Coordinates = new { alert.Latitude, alert.Longitude },
            alert.City,
            alert.CreatedAt,
            alert.Severity,
            User = AddHateoasLinks(alert.User),
            Links = new[]
            {
                new { Rel = "self", Href = Url.Action(nameof(GetAlert), new { id = alert.Id }) },
                new { Rel = "user", Href = Url.Action("GetUser", "Users", new { id = alert.UserId }) }
            }
        };
    }

    private IEnumerable<dynamic> AddHateoasLinks(IEnumerable<Alert> alerts)
    {
        return alerts.Select(alert => AddHateoasLinks(alert));
    }
}
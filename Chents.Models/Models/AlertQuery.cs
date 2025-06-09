namespace Chents.Models.Models;

public class AlertQuery
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string City { get; set; }
    public int? Days { get; set; }
    public double? RadiusKm { get; set; }
    public int? Limit { get; set; }
}
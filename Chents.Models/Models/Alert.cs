namespace Chents.Models.Models;

public class Alert
{
    public Guid Id { get; set; }
    public string Message { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string City { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid UserId { get; set; }
    public User User { get; set; }
    public AlertSeverity Severity { get; set; } = AlertSeverity.Medium;
}

public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}
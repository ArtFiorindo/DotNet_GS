using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Chents.AlertsService;
using Chents.Models;
using Chents.Models.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class AlertProcessingService : BackgroundService
{
    private readonly ILogger<AlertProcessingService> _logger;
    private readonly AlertsDbContext _dbContext;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string QueueName = "alerts_queue";

    public AlertProcessingService(ILogger<AlertProcessingService> logger, AlertsDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(
            queue: QueueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var alert = JsonSerializer.Deserialize<Alert>(message);

                _logger.LogInformation("Processing alert: {Message}", alert.Message);

                await _dbContext.Alerts.AddAsync(alert, stoppingToken);
                await _dbContext.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("Alert processed and saved: {Id}", alert.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing alert");
            }
        };

        _channel.BasicConsume(
            queue: QueueName,
            autoAck: true,
            consumer: consumer);
        
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
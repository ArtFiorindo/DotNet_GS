using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Chents.Models;
using Chents.Models.Models;

namespace Chents.API.Services;

public class AlertConsumerService : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string QueueName = "alerts_queue";

    public AlertConsumerService()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var alert = JsonSerializer.Deserialize<Alert>(message);

            // Process the alert (in a real app, this would be more complex)
            Console.WriteLine($"Received alert: {alert.Message} in {alert.City}");
        };

        _channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}
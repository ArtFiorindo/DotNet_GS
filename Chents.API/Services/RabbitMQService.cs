using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Chents.Models.Models;

namespace Chents.API.Services;

public class RabbitMQService : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string QueueName = "alerts_queue";

    public RabbitMQService()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public void PublishAlert(Alert alert)
    {
        var message = JsonSerializer.Serialize(alert);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: null, body: body);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
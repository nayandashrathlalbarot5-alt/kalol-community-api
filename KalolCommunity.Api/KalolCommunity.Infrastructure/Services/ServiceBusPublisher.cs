using Azure.Messaging.ServiceBus;
using KalolCommunity.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace KalolCommunity.Infrastructure.Services
{
    public class ServiceBusPublisher : IServiceBusPublisher
    {
        private readonly ServiceBusClient _client;
        private readonly string _queueName;

        public ServiceBusPublisher(IConfiguration configuration)
        {
            var connectionString = configuration["ServiceBus:ConnectionString"];
            _queueName = configuration["ServiceBus:QueueName"]
                ?? throw new ArgumentNullException("ServiceBus:QueueName");

            _client = new ServiceBusClient(connectionString
                ?? throw new ArgumentNullException("ServiceBus:ConnectionString"));
        }

        public async Task PublishAsync<T>(T message)
        {
            var sender = _client.CreateSender(_queueName);
            var json = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(json);

            await sender.SendMessageAsync(serviceBusMessage);
        }
    }
}

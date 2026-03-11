using Azure.Messaging.ServiceBus;
using KalolCommunity.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace KalolCommunity.Infrastructure.Services
{
    /// <summary>
    /// Publishes messages to Azure Service Bus queue.
    /// Used by the API to notify the NotificationWorker about user events.
    /// </summary>
    public class ServiceBusPublisher : IServiceBusPublisher
    {
        // Azure Service Bus client used to connect and send messages
        private readonly ServiceBusClient _client;

        // Name of the queue where messages will be sent (e.g. community-notifications)
        private readonly string _queueName;

        public ServiceBusPublisher(IConfiguration configuration)
        {
            // Read Service Bus connection string from appsettings.json
            var connectionString = configuration["ServiceBus:ConnectionString"];

            // Read the target queue name from appsettings.json
            // Throws if not configured so we catch missing config early at startup
            _queueName = configuration["ServiceBus:QueueName"]
                ?? throw new ArgumentNullException("ServiceBus:QueueName");

            // Create the Service Bus client using the connection string
            // Throws if connection string is missing
            _client = new ServiceBusClient(connectionString
                ?? throw new ArgumentNullException("ServiceBus:ConnectionString"));
        }

        /// <summary>
        /// Serializes the given message object to JSON and sends it to the Service Bus queue.
        /// </summary>
        public async Task SendMessageAsync<T>(T message)
        {
            // Create a sender that targets the configured queue
            var sender = _client.CreateSender(_queueName);

            // Serialize the message object to JSON string
            var json = JsonSerializer.Serialize(message);

            // Wrap the JSON string into a Service Bus message
            var serviceBusMessage = new ServiceBusMessage(json)
            {
                ContentType = "application/json"
            };

            // Send the message to the queue
            await sender.SendMessageAsync(serviceBusMessage);
        }
    }
}

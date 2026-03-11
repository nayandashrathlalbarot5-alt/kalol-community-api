using Azure.Messaging.ServiceBus;
using KalolCommunity.Contracts.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KalolCommunity.NotificationWorker
{
    public class NotificationWorker : BackgroundService
    {
        private readonly ILogger<NotificationWorker> _logger;
        private readonly IConfiguration _configuration;
        private ServiceBusProcessor? _processor;
        private ServiceBusClient? _client;

        public NotificationWorker(
            ILogger<NotificationWorker> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(
                args.Exception,
                "Service Bus processing error. ErrorSource: {ErrorSource}, EntityPath: {EntityPath}, FullyQualifiedNamespace: {FullyQualifiedNamespace}",
                args.ErrorSource,
                args.EntityPath,
                args.FullyQualifiedNamespace);

            Console.WriteLine($"[NotificationWorker] Service Bus error: {args.Exception.Message}");
            return Task.CompletedTask;
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            Console.WriteLine($"[NotificationWorker] Message received: {body}");

            var message = JsonSerializer.Deserialize<UserNotificationEventDTO>(body);

            if (message?.EventType == "UserRegistered")
            {
                _logger.LogInformation("Send Registration Email to {Email}", message.Email);
                Console.WriteLine($"[NotificationWorker] Send Registration Email to {message.Email}");
            }

            if (message?.EventType == "UserApproved")
            {
                _logger.LogInformation("Send Welcome Email to {Email}", message.Email);
                Console.WriteLine($"[NotificationWorker] Send Welcome Email to {message.Email}");
            }

            await args.CompleteMessageAsync(args.Message);
            Console.WriteLine("[NotificationWorker] Message completed successfully.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification worker started.");
            Console.WriteLine("[NotificationWorker] Worker started. Waiting for messages...");

            var connectionString = _configuration["ServiceBus:ConnectionString"]
                ?? throw new ArgumentNullException("ServiceBus:ConnectionString");
            var queueName = _configuration["ServiceBus:QueueName"]
                ?? throw new ArgumentNullException("ServiceBus:QueueName");

            Console.WriteLine($"[NotificationWorker] Listening to queue: {queueName}");

            _client = new ServiceBusClient(connectionString);
            _processor = _client.CreateProcessor(queueName);

            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;

            await _processor.StartProcessingAsync(stoppingToken);

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Notification worker cancellation requested.");
                Console.WriteLine("[NotificationWorker] Cancellation requested.");
            }
            finally
            {
                if (_processor is not null)
                {
                    await _processor.StopProcessingAsync(CancellationToken.None);
                    await _processor.DisposeAsync();
                }

                if (_client is not null)
                {
                    await _client.DisposeAsync();
                }

                _logger.LogInformation("Notification worker stopped.");
                Console.WriteLine("[NotificationWorker] Worker stopped.");
            }
        }
    }
}

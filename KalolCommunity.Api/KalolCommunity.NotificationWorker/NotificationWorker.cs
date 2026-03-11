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
    /// <summary>
    /// Listens to the main "community-notifications" queue and routes
    /// each message to the correct downstream queue based on EventType.
    /// </summary>
    public class NotificationWorker : BackgroundService
    {
        private readonly ILogger<NotificationWorker> _logger;
        private readonly IConfiguration _configuration;

        // Reads incoming messages from community-notifications queue
        private ServiceBusProcessor? _processor;

        // Single shared client for all queue connections
        private ServiceBusClient? _client;

        // Sends UserRegistered messages to registration-email-queue
        private ServiceBusSender? _registrationEmailSender;

        // Sends UserApproved messages to approval-email-queue
        private ServiceBusSender? _approvalEmailSender;

        public NotificationWorker(
            ILogger<NotificationWorker> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Called when Service Bus encounters an error (e.g. connection lost, lock expired).
        /// Logs the error and returns without throwing so the worker keeps running.
        /// </summary>
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

        /// <summary>
        /// Called when a new message arrives in community-notifications queue.
        /// Deserializes JSON → routes to correct downstream queue → completes message.
        /// </summary>
        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            // Read raw JSON from the incoming message body
            var body = args.Message.Body.ToString();
            Console.WriteLine($"[NotificationWorker] Message received: {body}");

            // Deserialize JSON body into strongly-typed DTO
            var message = JsonSerializer.Deserialize<UserNotificationEventDTO>(body);

            // If message is null/invalid, skip and complete to avoid infinite retry
            if (message is null)
            {
                _logger.LogWarning("Received null or invalid message body. Skipping.");
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            if (message.EventType == "UserRegistered")
            {
                // Forward to registration-email-queue → Azure Function sends registration email
                _logger.LogInformation("Forwarding registration email event for {Email}", message.Email);
                Console.WriteLine($"[NotificationWorker] Forwarding Registration Email for {message.Email}");
                await ForwardMessageAsync(_registrationEmailSender!, body);
            }
            else if (message.EventType == "UserApproved")
            {
                // Forward to approval-email-queue → Azure Function sends approval email
                _logger.LogInformation("Forwarding approval email event for {Email}", message.Email);
                Console.WriteLine($"[NotificationWorker] Forwarding Approval Email for {message.Email}");
                await ForwardMessageAsync(_approvalEmailSender!, body);
            }
            else
            {
                // Unknown event type - log and skip without failing
                _logger.LogWarning("Unknown event type {EventType}. Skipping.", message.EventType);
            }

            // Mark message as done so it is removed from community-notifications queue
            await args.CompleteMessageAsync(args.Message);
            Console.WriteLine("[NotificationWorker] Message completed successfully.");
        }

        /// <summary>
        /// Forwards the original message body to a target downstream queue.
        /// </summary>
        private async Task ForwardMessageAsync(ServiceBusSender sender, string body)
        {
            // Wrap the raw JSON into a new outgoing Service Bus message
            var outgoing = new ServiceBusMessage(body)
            {
                ContentType = "application/json"
            };

            // Send to the target queue (registration or approval)
            await sender.SendMessageAsync(outgoing);
        }

        /// <summary>
        /// Entry point for the background worker.
        /// Reads config → creates Service Bus connections → starts listening → waits until shutdown.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification worker started.");
            Console.WriteLine("[NotificationWorker] Worker started. Waiting for messages...");

            // Read Service Bus settings from appsettings.json
            var connectionString = _configuration["ServiceBus:ConnectionString"]
                ?? throw new ArgumentNullException("ServiceBus:ConnectionString");

            // Main queue that the API publishes to
            var queueName = _configuration["ServiceBus:QueueName"]
                ?? throw new ArgumentNullException("ServiceBus:QueueName");

            // Downstream queue for registration email events
            var registrationEmailQueue = _configuration["ServiceBus:RegistrationEmailQueueName"]
                ?? throw new ArgumentNullException("ServiceBus:RegistrationEmailQueueName");

            // Downstream queue for approval email events
            var approvalEmailQueue = _configuration["ServiceBus:ApprovalEmailQueueName"]
                ?? throw new ArgumentNullException("ServiceBus:ApprovalEmailQueueName");

            Console.WriteLine($"[NotificationWorker] Listening to queue: {queueName}");

            // Create one shared client used for all senders and processor
            _client = new ServiceBusClient(connectionString);

            // Create senders for each downstream queue
            _registrationEmailSender = _client.CreateSender(registrationEmailQueue);
            _approvalEmailSender = _client.CreateSender(approvalEmailQueue);

            // Create processor that reads from community-notifications queue
            _processor = _client.CreateProcessor(queueName);

            // Register the message handler and error handler callbacks
            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;

            // Start processing incoming messages
            await _processor.StartProcessingAsync(stoppingToken);

            try
            {
                // Hold the worker alive indefinitely until host shutdown is requested
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal graceful shutdown - not an error
                _logger.LogInformation("Notification worker cancellation requested.");
                Console.WriteLine("[NotificationWorker] Cancellation requested.");
            }
            finally
            {
                // Stop accepting new messages before disposing
                if (_processor is not null)
                {
                    await _processor.StopProcessingAsync(CancellationToken.None);
                    await _processor.DisposeAsync();
                }

                // Dispose senders to release queue connections
                if (_registrationEmailSender is not null)
                    await _registrationEmailSender.DisposeAsync();

                if (_approvalEmailSender is not null)
                    await _approvalEmailSender.DisposeAsync();

                // Dispose client last after all senders/processors are released
                if (_client is not null)
                    await _client.DisposeAsync();

                _logger.LogInformation("Notification worker stopped.");
                Console.WriteLine("[NotificationWorker] Worker stopped.");
            }
        }
    }
}

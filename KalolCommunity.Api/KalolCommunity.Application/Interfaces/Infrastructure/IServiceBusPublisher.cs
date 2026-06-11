using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Application.Interfaces.Infrastructure
{
    public interface IServiceBusPublisher
    {
        Task SendMessageAsync<T>(T message);
        Task SendMessageAsync<T>(T message, string queueName);
        Task SendMessageAsync<T>(T message, string queueName, DateTimeOffset scheduledEnqueueTime);
    }
}

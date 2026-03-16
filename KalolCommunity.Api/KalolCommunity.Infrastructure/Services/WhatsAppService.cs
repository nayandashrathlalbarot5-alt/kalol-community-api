using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;


namespace KalolCommunity.Infrastructure.Services
{
    public class WhatsAppService
    {
        private readonly string accountSid = "AC269df5fe22a881bcd2fe69fb890c2786";
        private readonly string authToken = "d65d85bc43beeb74cfa77ff30015ad3e";

        public void SendMessage(string mobile, string name)
        {
            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
             from: new PhoneNumber("whatsapp:+14155238886"),
             to: new PhoneNumber($"whatsapp:+91{mobile}"),
             body: $"Hello {name}, your registration request has been received. Admin will review it soon."
         );
        }
    }
}
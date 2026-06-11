using KalolCommunity.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Infrastructure.Services
{
    public class Msg91OtpService : IMsg91OtpService
    {
        public Task<bool> SendOtpAsync(string mobileNumber)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyOtpAsync(string mobileNumber, string otp)
        {
            throw new NotImplementedException();
        }
    }
}

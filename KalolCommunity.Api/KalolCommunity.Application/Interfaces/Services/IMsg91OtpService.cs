using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Application.Interfaces.Services
{
    public interface IMsg91OtpService
    {
        Task<bool> SendOtpAsync(string mobileNumber);

        Task<bool> VerifyOtpAsync(string mobileNumber, string otp);
    }
}

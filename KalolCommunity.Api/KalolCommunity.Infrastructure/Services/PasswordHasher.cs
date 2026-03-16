using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KalolCommunity.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace KalolCommunity.Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<object> _hasher = new();
        private readonly ILogger<PasswordHasher> _logger;

        public PasswordHasher(ILogger<PasswordHasher> logger)
        {
            _logger = logger;
        }

        public string Hash(string password)
        {
            _logger.LogDebug("Hashing password");
            return _hasher.HashPassword(null, password);
        }

        public bool Verify(string hash, string password)
        {
            _logger.LogDebug("Verifying password hash");
            return _hasher.VerifyHashedPassword(null, hash, password)
                == PasswordVerificationResult.Success;
        }
    }
}

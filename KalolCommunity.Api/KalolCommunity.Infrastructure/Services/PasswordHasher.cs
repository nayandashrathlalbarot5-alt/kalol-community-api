using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KalolCommunity.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace KalolCommunity.Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<object> _hasher = new();

        public string Hash(string password)
            => _hasher.HashPassword(null, password);

        public bool Verify(string hash, string password)
            => _hasher.VerifyHashedPassword(null, hash, password)
            == PasswordVerificationResult.Success;
    }
}

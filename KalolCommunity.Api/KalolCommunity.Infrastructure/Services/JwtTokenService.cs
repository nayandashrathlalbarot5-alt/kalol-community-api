using KalolCommunity.Application.Interfaces.Infrastructure;
using KalolCommunity.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        // IConfiguration to access appsettings.json values like issuer and token expiry times
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtTokenService> _logger;

        // Constructor injects IConfiguration dependency
        public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateAccessToken(User user)
        {
            _logger.LogInformation("Generating access token for UserId: {UserId}", user.UserId);

            // Initialize JWT token handler which creates and serializes tokens
            var tokenHandler = new JwtSecurityTokenHandler();

            // Generate a new unique identifier for the JWT token (jti claim)
            string jwtId = Guid.NewGuid().ToString();

            var jwtSettings = _configuration.GetSection("JwtSettings");

            // Define the claims to be embedded in the JWT token
            var claims = new List<Claim>
            {
                // Subject claim represents user identifier
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),

                // User email claim for identification purposes
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                
                // User FullName claim for identification purposes
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                
                // JWT ID claim for unique token identification (used to link refresh tokens)
                new Claim(JwtRegisteredClaimNames.Jti, jwtId)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(jwtSettings["ExpiryMinutes"]!)
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Helper method to generate a secure random refresh token
        public string GenerateRefreshToken()
        {
            _logger.LogDebug("Generating refresh token");
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        // Helper method to hash tokens before storing them
        public string HashToken(string token)
        {
            //The refresh token is hashed using SHA256 before storing it in the database to prevent token theft from compromising security.
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(token)));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using KalolCommunity.Domain.Entities;
using KalolCommunity.Application.Interfaces;

namespace KalolCommunity.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        // IConfiguration to access appsettings.json values like issuer and token expiry times
        private readonly IConfiguration _configuration;

        // Constructor injects IConfiguration dependency
        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(User user)
        {
            // Initialize JWT token handler which creates and serializes tokens
            var tokenHandler = new JwtSecurityTokenHandler();

            // Generate a new unique identifier for the JWT token (jti claim)
            string jwtId = Guid.NewGuid().ToString();

            var jwtSettings = _configuration.GetSection("JwtSettings");

            // Define the claims to be embedded in the JWT token
            var claims = new List<Claim>
            {
                // Subject claim represents user identifier
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),

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
    }
}

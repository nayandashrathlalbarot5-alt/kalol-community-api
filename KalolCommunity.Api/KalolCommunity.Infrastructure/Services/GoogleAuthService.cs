using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using KalolCommunity.Contracts.DTO;
using KalolCommunity.Application.Interfaces;

namespace KalolCommunity.Api.ExternalIntegration
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        public GoogleAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<GoogleUserInfoDto> ValidateTokenAsync(string idToken)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { _configuration["GoogleAuth:ClientId"] }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleUserInfoDto
            {
                GoogleId = payload.Subject,
                Email = payload.Email,
                FullName = payload.Name,
                PictureUrl = payload.Picture
            };
        }
    }
}
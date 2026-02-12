namespace KalolCommunity.Application.Common
{
    public static class ResponseMessages
    {
        public const string EmailAlreadyRegistered = "Email is already registered.";
        public const string RegistrationSuccessful = "Registration successful";
        public const string LoginSuccessful = "Login successful";

        // Common error messages
        public const string InvalidCredentials = "Invalid email or password";
        public const string UserNotFound = "User not found";
        public const string InvalidUserId = "Invalid user id";
        public const string RefreshTokenNotFound = "Refresh token not found";
        public const string RefreshTokenExpiredOrRevoked = "Refresh token expired or revoked";
        public const string TokenRefreshed = "Access token refreshed successfully";
        public const string InvalidGoogleToken = "Invalid Google token";
    }
}

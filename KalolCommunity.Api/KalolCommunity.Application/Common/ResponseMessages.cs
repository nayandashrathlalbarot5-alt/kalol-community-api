namespace KalolCommunity.Application.Common
{
    public static class ResponseMessages
    {
        public const string EmailAlreadyRegistered = "Email is already registered.";
        public const string RegistrationSuccessful = "Registration successful";
        public const string LoginSuccessful = "Login successful";
        public const string InvalidCredentials = "Invalid email or password";
        public const string UserNotFound = "User not found";
        public const string InvalidUserId = "Invalid user id";
        public const string RefreshTokenNotFound = "Refresh token not found";
        public const string RefreshTokenExpiredOrRevoked = "Refresh token expired or revoked";
        public const string TokenRefreshed = "Access token refreshed successfully";
        public const string InvalidGoogleToken = "Invalid Google token";
        public const string CommunityDetailCreated = "Community details created successfully";
        public const string CommunityDetailUpdated = "Community details updated successfully";
        public const string CommunityDetailNotFound = "Community details not found";
        public const string CommunityEmailExists = "Email already exists";
        public const string CommunityPhoneExists = "Phone number already exists";
        public const string InvalidCountry = "Invalid country";
        public const string InvalidState = "Invalid state";
        public const string InvalidDateOfBirth = "Date of birth must be in the past";
        public const string Forbidden = "Access denied";
        public const string ProfileDetailRetrieved = "Profile details retrieved successfully";
        public const string ProfileDetailNotFound = "Profile details not found";
        
        // Blob Storage Messages
        public const string FileUploadedSuccessfully = "File uploaded successfully";
        public const string NoFileProvided = "No file provided";
        public const string FileDeletedSuccessfully = "Temporary file deleted successfully";
        public const string FileNameRequired = "File name is required";
        public const string FileUploadError = "An error occurred while uploading the file";
        public const string FileDeleteError = "An error occurred while deleting the file";
        public const string TemporaryFileReadyForSubmission = "Temporary file ready for final submission";
        
        // Master Data Messages
        public const string CountriesRetrievedSuccessfully = "Countries retrieved successfully";
        public const string StatesRetrievedSuccessfully = "States retrieved successfully";
    }
}

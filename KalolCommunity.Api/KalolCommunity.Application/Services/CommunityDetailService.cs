using System;
using System.Net;
using System.Threading.Tasks;
using KalolCommunity.Application.Common;
using KalolCommunity.Application.Interfaces;
using KalolCommunity.Contracts.DTO;
using KalolCommunity.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace KalolCommunity.Application.Services
{
    public class CommunityDetailService : ICommunityDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBlobService _blobService;
        private readonly ILogger<CommunityDetailService> _logger;

        public CommunityDetailService(
            IUnitOfWork unitOfWork,
            ILogger<CommunityDetailService> logger,
            IBlobService blobService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _blobService = blobService;
        }

        public async Task<ApiResponse<CommunityRequestDTO>> CreateAsync(Guid userId, CommunityRequestDTO dto)
        {
            if (userId == Guid.Empty)
            {
                return InvalidResponse(ResponseMessages.InvalidUserId, HttpStatusCode.BadRequest);
            }

            if (dto.DateOfBirth.Date >= DateTime.UtcNow.Date)
            {
                return InvalidResponse(ResponseMessages.InvalidDateOfBirth, HttpStatusCode.BadRequest);
            }

            if (!await _unitOfWork.Countries.AnyAsync(c => c.CountryId == dto.Country))
            {
                return InvalidResponse(ResponseMessages.InvalidCountry, HttpStatusCode.BadRequest);
            }

            if (!await _unitOfWork.States.AnyAsync(s => s.StateId == dto.State))
            {
                return InvalidResponse(ResponseMessages.InvalidState, HttpStatusCode.BadRequest);
            }

            string normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            if (await _unitOfWork.CommunityDetails.AnyAsync(c => c.Email == normalizedEmail))
            {
                return InvalidResponse(ResponseMessages.CommunityEmailExists, HttpStatusCode.Conflict);
            }

            if (await _unitOfWork.CommunityDetails.AnyAsync(c => c.PrimatyContactNumber == dto.PrimaryContactNumber))
            {
                return InvalidResponse(ResponseMessages.CommunityPhoneExists, HttpStatusCode.Conflict);
            }

            var entity = new CommunityDetail
            {
                UserId = userId,
                FirstName = dto.FirstName.Trim(),
                MiddleName = dto.MiddleName.Trim(),
                LastName = dto.LastName.Trim(),
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                MaritalStatus = dto.MaritalStatus,
                BloodGroup = dto.BloodGroup,
                Email = normalizedEmail,
                PrimatyContactNumber = dto.PrimaryContactNumber,
                AlternateContactNumber = string.IsNullOrWhiteSpace(dto.AlternateContactNumber) ? null : dto.AlternateContactNumber,
                IsWhatsappPrimary = dto.IsWhatsappPrimary,
                IsWhatsappAlternate = dto.IsWhatsappAlternate,
                PhotoPath = string.IsNullOrWhiteSpace(dto.PhotoPath)
                            ? null : await _blobService.MoveToPermanentAsync(dto.PhotoPath),
                Occupation = dto.Occupation,
                Education = dto.Education,
                FatherName = dto.FatherName.Trim(),
                MotherName = dto.MotherName.Trim(),
                SpouseName = string.IsNullOrWhiteSpace(dto.SpouseName) ? null : dto.SpouseName.Trim(),
                NumberOfChildren = dto.NumberOfChildren,
                NumberOfSons = dto.NumberOfSons,
                NumberOfDaughters = dto.NumberOfDaughters,
                CurrentAddress = dto.CurrentAddress.Trim(),
                CountryId = dto.Country,
                StateId = dto.State,
                City = dto.City.Trim(),
                PinCode = dto.PinCode.Trim(),
                PermanentAddress = string.IsNullOrWhiteSpace(dto.PermanentAddress) ? null : dto.PermanentAddress.Trim(),
                ProfessionType = dto.ProfessionType?.Trim(),
                BusinessType = string.IsNullOrWhiteSpace(dto.BusinessType) ? null : dto.BusinessType.Trim(),
                CompanyName = string.IsNullOrWhiteSpace(dto.CompanyName) ? null : dto.CompanyName.Trim(),
                Skills = string.IsNullOrWhiteSpace(dto.Skills) ? null : dto.Skills.Trim(),
                OtherDetails = string.IsNullOrWhiteSpace(dto.OtherDetails) ? null : dto.OtherDetails.Trim(),
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsApproved = false
            };

            await _unitOfWork.CommunityDetails.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Community detail created for user {UserId}", userId);

            var resultDto = MapToDto(entity);
            // Attach runtime URL (SAS) if photo exists
            if (!string.IsNullOrWhiteSpace(entity.PhotoPath))
            {
                resultDto.PhotoUrl = BuildPhotoUrl(entity.PhotoPath);
            }

            return new ApiResponse<CommunityRequestDTO>
            {
                Success = true,
                Message = ResponseMessages.CommunityDetailCreated,
                StatusCode = (int)HttpStatusCode.Created,
                Data = resultDto
            };
        }

        public async Task<ApiResponse<CommunityRequestDTO>> UpdateAsync(Guid userId, int communityDetailId, CommunityRequestDTO dto)
        {
            var entity = await _unitOfWork.CommunityDetails.GetAsync(c => c.Id == communityDetailId);

            if (entity == null)
            {
                return InvalidResponse(ResponseMessages.CommunityDetailNotFound, HttpStatusCode.NotFound);
            }

            if (entity.UserId != userId)
            {
                return InvalidResponse(ResponseMessages.Forbidden, HttpStatusCode.Forbidden);
            }

            if (dto.DateOfBirth.Date >= DateTime.UtcNow.Date)
            {
                return InvalidResponse(ResponseMessages.InvalidDateOfBirth, HttpStatusCode.BadRequest);
            }

            if (!await _unitOfWork.Countries.AnyAsync(c => c.CountryId == dto.Country))
            {
                return InvalidResponse(ResponseMessages.InvalidCountry, HttpStatusCode.BadRequest);
            }

            if (!await _unitOfWork.States.AnyAsync(s => s.StateId == dto.State))
            {
                return InvalidResponse(ResponseMessages.InvalidState, HttpStatusCode.BadRequest);
            }

            string normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            if (await _unitOfWork.CommunityDetails.AnyAsync(c => c.Email == normalizedEmail && c.Id != communityDetailId))
            {
                return InvalidResponse(ResponseMessages.CommunityEmailExists, HttpStatusCode.Conflict);
            }

            if (await _unitOfWork.CommunityDetails.AnyAsync(c => c.PrimatyContactNumber == dto.PrimaryContactNumber && c.Id != communityDetailId))
            {
                return InvalidResponse(ResponseMessages.CommunityPhoneExists, HttpStatusCode.Conflict);
            }

            entity.FirstName = dto.FirstName.Trim();
            entity.MiddleName = dto.MiddleName.Trim();
            entity.LastName = dto.LastName.Trim();
            entity.DateOfBirth = dto.DateOfBirth;
            entity.Gender = dto.Gender;
            entity.MaritalStatus = dto.MaritalStatus;
            entity.BloodGroup = dto.BloodGroup;
            entity.Email = normalizedEmail;
            entity.PrimatyContactNumber = dto.PrimaryContactNumber;
            entity.AlternateContactNumber = string.IsNullOrWhiteSpace(dto.AlternateContactNumber) ? null : dto.AlternateContactNumber;
            
            // Handle photo update: move from temp to permanent if a new photo is provided
            if (!string.IsNullOrWhiteSpace(dto.PhotoPath))
            {
                // If a new photo is provided and different from the stored one, move it to permanent
                if (dto.PhotoPath != entity.PhotoPath)
                {
                    entity.PhotoPath = await _blobService.MoveToPermanentAsync(dto.PhotoPath);
                }
            }
            else
            {
                // If no photo provided in update, keep the existing one
                // entity.PhotoPath remains unchanged
            }
            
            entity.Occupation = dto.Occupation;
            entity.Education = dto.Education;
            entity.FatherName = dto.FatherName.Trim();
            entity.MotherName = dto.MotherName.Trim();
            entity.SpouseName = string.IsNullOrWhiteSpace(dto.SpouseName) ? null : dto.SpouseName.Trim();
            entity.NumberOfChildren = dto.NumberOfChildren;
            entity.NumberOfSons = dto.NumberOfSons;
            entity.NumberOfDaughters = dto.NumberOfDaughters;
            entity.CurrentAddress = dto.CurrentAddress.Trim();
            entity.CountryId = dto.Country;
            entity.StateId = dto.State;
            entity.City = dto.City.Trim();
            entity.PinCode = dto.PinCode.Trim();
            entity.PermanentAddress = string.IsNullOrWhiteSpace(dto.PermanentAddress) ? null : dto.PermanentAddress.Trim();
            entity.ProfessionType = dto.ProfessionType?.Trim();
            entity.BusinessType = string.IsNullOrWhiteSpace(dto.BusinessType) ? null : dto.BusinessType.Trim();
            entity.CompanyName = string.IsNullOrWhiteSpace(dto.CompanyName) ? null : dto.CompanyName.Trim();
            entity.Skills = string.IsNullOrWhiteSpace(dto.Skills) ? null : dto.Skills.Trim();
            entity.OtherDetails = string.IsNullOrWhiteSpace(dto.OtherDetails) ? null : dto.OtherDetails.Trim();
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedByUserId = userId;

            await _unitOfWork.CommunityDetails.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Community detail updated for user {UserId}, id {CommunityDetailId}", userId, communityDetailId);

            var resultDto = MapToDto(entity);
            if (!string.IsNullOrWhiteSpace(entity.PhotoPath))
            {
                resultDto.PhotoUrl = BuildPhotoUrl(entity.PhotoPath);
            }

            return new ApiResponse<CommunityRequestDTO>
            {
                Success = true,
                Message = ResponseMessages.CommunityDetailUpdated,
                StatusCode = (int)HttpStatusCode.OK,
                Data = resultDto
            };
        }

        public async Task<ApiResponse<CommunityRequestDTO>> GetByUserIdAsync(Guid userId)
        {
            var entity = await _unitOfWork.CommunityDetails.GetAsync(c => c.UserId == userId, asNoTracking: true);

            if (entity == null)
            {
                return InvalidResponse(ResponseMessages.ProfileDetailNotFound, HttpStatusCode.NotFound);
            }

            var resultDto = MapToDto(entity);
            if (!string.IsNullOrWhiteSpace(entity.PhotoPath))
            {
                resultDto.PhotoUrl = BuildPhotoUrl(entity.PhotoPath);
            }

            return new ApiResponse<CommunityRequestDTO>
            {
                Success = true,
                Message = ResponseMessages.ProfileDetailRetrieved,
                StatusCode = (int)HttpStatusCode.OK,
                Data = resultDto
            };
        }

        private static CommunityRequestDTO MapToDto(CommunityDetail entity)
        {
            return new CommunityRequestDTO
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                MiddleName = entity.MiddleName,
                LastName = entity.LastName,
                DateOfBirth = entity.DateOfBirth,
                Gender = entity.Gender,
                MaritalStatus = entity.MaritalStatus,
                BloodGroup = entity.BloodGroup,
                Email = entity.Email,
                PrimaryContactNumber = entity.PrimatyContactNumber,
                IsWhatsappPrimary = entity.IsWhatsappPrimary,
                AlternateContactNumber = entity.AlternateContactNumber,
                IsWhatsappAlternate = entity.IsWhatsappAlternate,
                PhotoPath = entity.PhotoPath,
                Occupation = entity.Occupation,
                Education = entity.Education,
                FatherName = entity.FatherName,
                MotherName = entity.MotherName,
                SpouseName = entity.SpouseName,
                NumberOfChildren = entity.NumberOfChildren,
                NumberOfSons = entity.NumberOfSons,
                NumberOfDaughters = entity.NumberOfDaughters,
                CurrentAddress = entity.CurrentAddress,
                Country = entity.CountryId,
                State = entity.StateId,
                City = entity.City,
                PinCode = entity.PinCode,
                PermanentAddress = entity.PermanentAddress,
                ProfessionType = entity.ProfessionType,
                BusinessType = entity.BusinessType,
                CompanyName = entity.CompanyName,
                Skills = entity.Skills,
                OtherDetails = entity.OtherDetails
            };
        }

        /// <summary>
        /// Build a browser-usable URL from a stored PhotoPath.
        /// Supports both legacy full-URI stored values and filename-only values.
        /// </summary>
        private string? BuildPhotoUrl(string photoPath)
        {
            try
            {
                // If the DB already contains a full absolute URI, extract filename from it (last segment)
                if (Uri.IsWellFormedUriString(photoPath, UriKind.Absolute))
                {
                    var uri = new Uri(photoPath);
                    var lastSegment = uri.Segments.LastOrDefault();
                    if (!string.IsNullOrWhiteSpace(lastSegment))
                    {
                        // Trim possible leading '/'
                        var fileName = lastSegment.Trim('/');
                        return _blobService.GetPermanentBlobSasUrl(fileName, TimeSpan.FromHours(1));
                    }

                    // Fallback: try using entire path as filename
                    return _blobService.GetPermanentBlobSasUrl(photoPath, TimeSpan.FromHours(1));
                }

                // Otherwise assume the DB stores just the filename (recommended)
                return _blobService.GetPermanentBlobSasUrl(photoPath, TimeSpan.FromHours(1));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to build photo URL for path {PhotoPath}", photoPath);
                return null;
            }
        }

        private static ApiResponse<CommunityRequestDTO> InvalidResponse(string message, HttpStatusCode statusCode)
        {
            return new ApiResponse<CommunityRequestDTO>
            {
                Success = false,
                Message = message,
                StatusCode = (int)statusCode,
                Data = null
            };
        }
    }
}
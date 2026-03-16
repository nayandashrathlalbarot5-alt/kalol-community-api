using System;
using System.Net;
using System.Threading.Tasks;
using KalolCommunity.Application.Common;
using KalolCommunity.Application.Interfaces;
using KalolCommunity.Application.Exceptions;
using KalolCommunity.Contracts.DTO;
using KalolCommunity.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace KalolCommunity.Application.Services
{
    public class CommunityDetailService : ICommunityDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBlobService _blobService;
        private readonly ILogger<CommunityDetailService> _logger;
        private readonly IServiceBusPublisher _serviceBusSender;
        private readonly string _registrationEmailQueueName;
        private readonly string _registrationWhatsAppQueueName;

        public CommunityDetailService(
            IUnitOfWork unitOfWork,
            ILogger<CommunityDetailService> logger,
            IBlobService blobService,
            IServiceBusPublisher serviceBusSender,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _blobService = blobService;
            _serviceBusSender = serviceBusSender;
            _registrationEmailQueueName = configuration["ServiceBus:RegistrationEmailQueueName"]
                ?? throw new ArgumentNullException("ServiceBus:RegistrationEmailQueueName");
            _registrationWhatsAppQueueName = configuration["ServiceBus:RegistrationWhatsAppQueueName"]
                ?? throw new ArgumentNullException("ServiceBus:RegistrationWhatsAppQueueName");
        }

        public async Task<ApiResponse<CommunityRequestDTO>> CreateAsync(Guid userId, CommunityRequestDTO dto)
        {
            if (userId == Guid.Empty)
            {
                throw new BadRequestException(ResponseMessages.InvalidUserId);
            }

            if (dto.DateOfBirth.Date >= DateTime.UtcNow.Date)
            {
                throw new BadRequestException(ResponseMessages.InvalidDateOfBirth);
            }

            if (!await _unitOfWork.Countries.AnyAsync(c => c.CountryId == dto.Country))
            {
                throw new BadRequestException(ResponseMessages.InvalidCountry);
            }

            if (!await _unitOfWork.States.AnyAsync(s => s.StateId == dto.State))
            {
                throw new BadRequestException(ResponseMessages.InvalidState);
            }

            var normalizedEmail = NormalizeOptionalEmail(dto.Email);

            if (!string.IsNullOrWhiteSpace(normalizedEmail) &&
                await _unitOfWork.CommunityDetails.AnyAsync(c => c.Email == normalizedEmail))         {
                throw new ConflictException(ResponseMessages.CommunityEmailExists);
            }

            if (await _unitOfWork.CommunityDetails.AnyAsync(c => c.PrimatyContactNumber == dto.PrimaryContactNumber))
            {
                throw new ConflictException(ResponseMessages.CommunityPhoneExists);
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
                Education = dto.Education,
                FatherName = dto.FatherName.Trim(),
                MotherName = dto.MotherName.Trim(),
                SpouseName = string.IsNullOrWhiteSpace(dto.SpouseName) ? null : dto.SpouseName.Trim(),
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

            // Save children details if provided
            if (dto.Children != null && dto.Children.Count > 0)
            {
                foreach (var child in dto.Children)
                {
                    var childEntity = new ChildrenDetail
                    {
                        UserId = userId,
                        CommunityDetailId = entity.Id,
                        ChildName = child.ChildName.Trim(),
                        Gender = child.Gender,
                        MaritalStatus = child.MaritalStatus,
                        Address = string.IsNullOrWhiteSpace(child.Address) ? null : child.Address.Trim(),
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _unitOfWork.ChildrenDetails.AddAsync(childEntity);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            _logger.LogInformation("Community detail created for user {UserId}", userId);

            var resultDto = MapToDto(entity);
            // Attach runtime URL (SAS) if photo exists
            if (!string.IsNullOrWhiteSpace(entity.PhotoPath))
            {
                resultDto.PhotoUrl = BuildPhotoUrl(entity.PhotoPath);
            }

            var notificationEvent = new UserNotificationEventDTO
            {
                UserId = userId,
                Name = string.Join(" ", new[] { entity.FirstName, entity.MiddleName, entity.LastName }
                    .Where(x => !string.IsNullOrWhiteSpace(x))),
                Email = entity.Email,
                Mobile = entity.IsWhatsappPrimary ? entity.PrimatyContactNumber : entity.AlternateContactNumber,
                EventType = "UserRegistered"
            };

            // Publish to both email and WhatsApp queues for asynchronous processing
            await _serviceBusSender.SendMessageAsync(notificationEvent, _registrationEmailQueueName);
            await _serviceBusSender.SendMessageAsync(notificationEvent, _registrationWhatsAppQueueName);

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
                throw new NotFoundException(ResponseMessages.CommunityDetailNotFound);
            }

            if (entity.UserId != userId)
            {
                throw new ForbiddenException(ResponseMessages.Forbidden);
            }

            if (dto.DateOfBirth.Date >= DateTime.UtcNow.Date)
            {
                throw new BadRequestException(ResponseMessages.InvalidDateOfBirth);
            }

            if (!await _unitOfWork.Countries.AnyAsync(c => c.CountryId == dto.Country))
            {
                throw new BadRequestException(ResponseMessages.InvalidCountry);
            }

            if (!await _unitOfWork.States.AnyAsync(s => s.StateId == dto.State))
            {
                throw new BadRequestException(ResponseMessages.InvalidState);
            }

            var normalizedEmail = NormalizeOptionalEmail(dto.Email);

            if (!string.IsNullOrWhiteSpace(normalizedEmail) &&
                await _unitOfWork.CommunityDetails.AnyAsync(c => c.Email == normalizedEmail && c.Id != communityDetailId))
            {
                throw new ConflictException(ResponseMessages.CommunityEmailExists);
            }

            if (await _unitOfWork.CommunityDetails.AnyAsync(c => c.PrimatyContactNumber == dto.PrimaryContactNumber && c.Id != communityDetailId))
            {
                throw new ConflictException(ResponseMessages.CommunityPhoneExists);
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
            if (!string.IsNullOrWhiteSpace(dto.PhotoPath) && dto.IsPhotoUploading)
            {
                // If a new photo is provided and different from the stored one, move it to permanent
                if (dto.PhotoPath != entity.PhotoPath)
                {
                    entity.PhotoPath = await _blobService.MoveToPermanentAsync(dto.PhotoPath);
                }
            }

            entity.Education = dto.Education;
            entity.FatherName = dto.FatherName.Trim();
            entity.MotherName = dto.MotherName.Trim();
            entity.SpouseName = string.IsNullOrWhiteSpace(dto.SpouseName) ? null : dto.SpouseName.Trim();
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

            // Handle children details updates
            if (dto.Children != null && dto.Children.Count > 0)
            {
                // Get existing children for this community detail
                foreach (var childDto in dto.Children)
                {
                    if (childDto.Id.HasValue && childDto.Id.Value > 0)
                    {
                        // Update existing child
                        var existingChild = await _unitOfWork.ChildrenDetails.GetAsync(c => c.Id == childDto.Id.Value);
                        if (existingChild != null && existingChild.CommunityDetailId == communityDetailId)
                        {
                            existingChild.ChildName = childDto.ChildName.Trim();
                            existingChild.Gender = childDto.Gender;
                            existingChild.MaritalStatus = childDto.MaritalStatus;
                            existingChild.Address = string.IsNullOrWhiteSpace(childDto.Address) ? null : childDto.Address.Trim();
                            existingChild.UpdatedDate = DateTime.UtcNow;
                            await _unitOfWork.ChildrenDetails.UpdateAsync(existingChild);
                        }
                    }
                    else
                    {
                        // Add new child
                        var newChild = new ChildrenDetail
                        {
                            UserId = userId,
                            CommunityDetailId = communityDetailId,
                            ChildName = childDto.ChildName.Trim(),
                            Gender = childDto.Gender,
                            MaritalStatus = childDto.MaritalStatus,
                            Address = string.IsNullOrWhiteSpace(childDto.Address) ? null : childDto.Address.Trim(),
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        };
                        await _unitOfWork.ChildrenDetails.AddAsync(newChild);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }

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
            // Use the new method that eagerly loads children
            var entity = await _unitOfWork.CommunityDetails.GetByUserIdWithChildrenAsync(userId);

            if (entity == null)
            {
                throw new NotFoundException(ResponseMessages.ProfileDetailNotFound);
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
                Education = entity.Education,
                FatherName = entity.FatherName,
                MotherName = entity.MotherName,
                SpouseName = entity.SpouseName,
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
                OtherDetails = entity.OtherDetails,
                Children = entity.ChildrenDetails?.Select(c => new ChildrenDetailRequestDTO
                {
                    Id = c.Id,
                    ChildName = c.ChildName,
                    Gender = c.Gender,
                    MaritalStatus = c.MaritalStatus,
                    Address = c.Address
                }).ToList() ?? new List<ChildrenDetailRequestDTO>()
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

        private static string? NormalizeOptionalEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            return email.Trim().ToLowerInvariant();
        }
    }
}
using System;
using System.Net;
using System.Threading.Tasks;
using KalolCommunity.Application.Common;
using KalolCommunity.Application.Interfaces.Repositories;
using KalolCommunity.Application.Interfaces.Services;
using KalolCommunity.Contracts.DTO;
using KalolCommunity.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace KalolCommunity.Application.Services
{
    public class ContactUsService : IContactUsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ContactUsService> _logger;

        public ContactUsService(
            IUnitOfWork unitOfWork,
            ILogger<ContactUsService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<ContactUsResponseDTO>> CreateAsync(ContactUsRequestDTO dto)
        {
            var entity = new ContactUs
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                Message = dto.Message.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _unitOfWork.ContactUs.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("ContactUs record created successfully. Id: {Id}, Email: {Email}", entity.Id, entity.Email);

            return new ApiResponse<ContactUsResponseDTO>
            {
                Success = true,
                Message = ResponseMessages.ContactUsCreated,
                StatusCode = (int)HttpStatusCode.Created,
                Data = new ContactUsResponseDTO
                {
                    Id = entity.Id,
                    FirstName = entity.FirstName,
                    LastName = entity.LastName,
                    PhoneNumber = entity.PhoneNumber,
                    Email = entity.Email,
                    Message = entity.Message,
                    CreatedAt = entity.CreatedAt
                }
            };
        }
    }
}

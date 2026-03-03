using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using KalolCommunity.Application.Common;
using KalolCommunity.Application.Interfaces;
using KalolCommunity.Contracts.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KalolCommunity.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CommunityController : ControllerBase
    {
        private readonly ICommunityDetailService _communityDetailService;

        public CommunityController(ICommunityDetailService communityDetailService)
        {
            _communityDetailService = communityDetailService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CommunityRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                string errorMessage = string.Join(" | ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                return BadRequest(new ApiResponse<CommunityRequestDTO>
                {
                    Success = false,
                    Message = errorMessage,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null
                });
            }

            var authError = ValidateUserAndGetId(out Guid userId);
            if (authError != null)
                return authError;

            var result = await _communityDetailService.CreateAsync(userId, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, CommunityRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                string errorMessage = string.Join(" | ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                return BadRequest(new ApiResponse<CommunityRequestDTO>
                {
                    Success = false,
                    Message = errorMessage,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null
                });
            }

            var authError = ValidateUserAndGetId(out Guid userId);
            if (authError != null)
                return authError;

            var result = await _communityDetailService.UpdateAsync(userId, id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("profile/details")]
        public async Task<IActionResult> GetProfileDetailsByUserId()
        {
            var authError = ValidateUserAndGetId(out Guid userId);
            if (authError != null)
                return authError;

            var result = await _communityDetailService.GetByUserIdAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        private IActionResult? ValidateUserAndGetId(out Guid userId)
        {
            if (TryGetUserId(out userId))
            {
                return null;
            }

            return Unauthorized(new ApiResponse<CommunityRequestDTO>
            {
                Success = false,
                Message = ResponseMessages.InvalidUserId,
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Data = null
            });
        }

        private bool TryGetUserId(out Guid userId)
        {
            userId = Guid.Empty;

            // First try the standard "sub" claim (best practice)
            string? userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            // If not found, try the "nameidentifier" claim (fallback for your current token)
            if (string.IsNullOrEmpty(userIdValue))
            {
                userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            // If still not found, user is not properly authenticated
            if (string.IsNullOrEmpty(userIdValue))
            {
                return false;
            }

            // Parse the claim value to Guid
            return Guid.TryParse(userIdValue, out userId);
        }
    }
}
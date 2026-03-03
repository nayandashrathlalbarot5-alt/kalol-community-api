using System;
using System.Threading.Tasks;
using KalolCommunity.Application.Common;
using KalolCommunity.Contracts.DTO;

namespace KalolCommunity.Application.Interfaces
{
    public interface ICommunityDetailService
    {
        Task<ApiResponse<CommunityRequestDTO>> CreateAsync(Guid userId, CommunityRequestDTO dto);
        Task<ApiResponse<CommunityRequestDTO>> UpdateAsync(Guid userId, int communityDetailId, CommunityRequestDTO dto);
        Task<ApiResponse<CommunityRequestDTO>> GetByUserIdAsync(Guid userId);
    }
}
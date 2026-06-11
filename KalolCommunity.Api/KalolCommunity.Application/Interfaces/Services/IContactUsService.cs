using System.Threading.Tasks;
using KalolCommunity.Application.Common;
using KalolCommunity.Contracts.DTO;

namespace KalolCommunity.Application.Interfaces.Services
{
    public interface IContactUsService
    {
        Task<ApiResponse<ContactUsResponseDTO>> CreateAsync(ContactUsRequestDTO dto);
    }
}

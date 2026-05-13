using System.Threading.Tasks;
using KalolCommunity.Application.Common;
using KalolCommunity.Contracts.DTO;

namespace KalolCommunity.Application.Interfaces
{
    public interface IContactUsService
    {
        Task<ApiResponse<ContactUsResponseDTO>> CreateAsync(ContactUsRequestDTO dto);
    }
}

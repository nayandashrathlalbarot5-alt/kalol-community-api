using System.ComponentModel.DataAnnotations;

namespace KalolCommunity.Contracts.DTO
{
    public class SendOtpRequestDTO
    {
        public string Email { get; set; } = null!;
        public string Flag { get; set; } = null!;
    }
}

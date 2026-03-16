using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Contracts.DTO
{
    public class ChildrenDetailRequestDTO
    {
        public int? Id { get; set; } // Optional, for updates

        [Required]
        [MaxLength(100)]
        public string ChildName { get; set; } = null!;

        [Required]
        public string Gender { get; set; } = null!;

        [Required]
        public string MaritalStatus { get; set; } = null!;

        [MaxLength(500)]
        public string? Address { get; set; }
    }
}

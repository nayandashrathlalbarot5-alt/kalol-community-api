using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Domain.Entities
{
    [Table("TblChildrenDetails")]
    public class ChildrenDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int CommunityDetailId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ChildName { get; set; } = null!;

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string MaritalStatus { get; set; } = null!;

        [MaxLength(500)]
        public string? Address { get; set; }

        public User? User { get; set; }

        public CommunityDetail? CommunityDetail { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KalolCommunity.Domain.Entities
{
    [Table("TblCommunityDetails")]
    public class CommunityDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string MiddleName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = null!;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(20)]
        public string Gender { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string MaritalStatus { get; set; } = null!;

        [MaxLength(5)]
        public string? BloodGroup { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [Required]
        [MaxLength(10)]
        public string? PrimatyContactNumber { get; set; }

        [MaxLength(10)]
        public string? AlternateContactNumber { get; set; } = null;

        [MaxLength(500)]
        public string? PhotoPath { get; set; }

        [Required]
        [MaxLength(50)]
        public string Education { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string FatherName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string MotherName { get; set; } = null!;

        [MaxLength(100)]
        public string? SpouseName { get; set; }

        [Required]
        [MaxLength(500)]
        public string CurrentAddress { get; set; } = null!;

        [Required]
        public int CountryId { get; set; }

        [Required]
        public int StateId { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = null!;

        [Required]
        [MaxLength(10)]
        public string PinCode { get; set; } = null!;

        [MaxLength(500)]
        public string? PermanentAddress { get; set; }

        [MaxLength(100)]
        public string? ProfessionType { get; set; }

        [MaxLength(100)]
        public string? BusinessType { get; set; }

        [MaxLength(150)]
        public string? CompanyName { get; set; }

        [MaxLength(150)]
        public string? Skills { get; set; }

        [MaxLength(500)]
        public string? OtherDetails { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsApproved { get; set; } = true;

        public bool IsWhatsappPrimary { get; set; } = true;

        public bool IsWhatsappAlternate { get; set; } = false;
        
        public Country? Country { get; set; }
        public State? State { get; set; }
        public User? User { get; set; }
        public ICollection<ChildrenDetail> ChildrenDetails { get; set; } = new List<ChildrenDetail>();
    }
}
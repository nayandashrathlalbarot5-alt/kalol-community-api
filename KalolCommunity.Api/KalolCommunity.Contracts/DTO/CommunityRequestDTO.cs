using System;
using System.ComponentModel.DataAnnotations;

namespace KalolCommunity.Contracts.DTO
{
    public class CommunityRequestDTO
    {
        public int Id { get; set; }

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
        public string Gender { get; set; } = null!;

        [Required]        
        public string MaritalStatus { get; set; } = null!;
        
        public string? BloodGroup { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Primary contact number must be exactly 10 digits")]
        public string PrimaryContactNumber { get; set; }

        [RegularExpression("^\\d{10}$", ErrorMessage = "Alternate contact number must be exactly 10 digits")]
        public string? AlternateContactNumber { get; set; }

        [MaxLength(500)]
        public string? PhotoPath { get; set; }

        public string? PhotoUrl { get; set; }

        [Required]        
        public string Occupation { get; set; } = null!;

        [Required]       
        public string Education { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string FatherName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string MotherName { get; set; } = null!;

        [MaxLength(100)]
        public string? SpouseName { get; set; }

        [Range(0, int.MaxValue)]
        public int? NumberOfChildren { get; set; }

        [Range(0, int.MaxValue)]
        public int? NumberOfSons { get; set; }

        [Range(0, int.MaxValue)]
        public int? NumberOfDaughters { get; set; }

        [Required]
        [MaxLength(500)]
        public string CurrentAddress { get; set; } = null!;

        [Required]
        public int Country { get; set; }

        [Required]
        public int State { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = null!;

        [Required]        
        public string PinCode { get; set; } = null!;

        [MaxLength(500)]
        public string? PermanentAddress { get; set; }

        [MaxLength(100)]
        public string? ProfessionType { get; set; }

        [MaxLength(150)]
        public string? CompanyName { get; set; }

        [MaxLength(100)]
        public string? BusinessType { get; set; }

        [MaxLength(200)]
        public string? Skills { get; set; }

        [MaxLength(500)]
        public string? OtherDetails { get; set; }
        public bool IsWhatsappPrimary { get; set; }
        public bool IsWhatsappAlternate { get; set; } 
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KalolCommunity.Domain.Entities
{
    [Table("TblStates")]
    public class State
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StateId { get; set; }

        [Required]
        [StringLength(100)]
        public string StateName { get; set; }

        [Required]
        [StringLength(20)]
        public string StateCode { get; set; }

        [Required]
        [ForeignKey("Country")]
        public int CountryId { get; set; }

        // Navigation property
        public Country Country { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Domain.Entities
{
    [Table("TblCountries")]
    public class Country
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CountryId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CountryName { get; set; }


        [Required]
        [StringLength(2)]
        public string CountryCode { get; set; } = string.Empty;

        // Navigation property for States
        public ICollection<State> States { get; set; } = new List<State>();
    }
}

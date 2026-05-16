using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10439147_PRO7311_POE_Back.Models
{
    public enum ClientRegion
    {
        EMEA,           // Europe, Middle East & Africa
        APAC,           // Asia-Pacific
        NorthAmerica,   // North America
        LatinAmerica,   // Latin America & Caribbean
        MiddleEast,     // Middle East
        SubSaharanAfrica, // Sub-Saharan Africa
        NorthAfrica,    // North Africa
        CentralAsia,    // Central Asia
        SouthAsia,      // South Asia
        EastAsia,       // East Asia
        SoutheastAsia   // Southeast Asia
    }

    [Table("Clients")]
    public class Clients
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string ContactDetails { get; set; } = string.Empty;

        [Required]
        public ClientRegion Region { get; set; }

        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------
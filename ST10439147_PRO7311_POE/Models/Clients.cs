using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10439147_PRO7311_POE.Models
{
    [Table("Clients")]
    public class Clients
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string ContactDetails { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Region { get; set; } = string.Empty;

        public ICollection<Contracts> Contracts { get; set; } = new List<Contracts>();

    }
}

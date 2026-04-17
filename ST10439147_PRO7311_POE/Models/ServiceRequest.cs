using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace ST10439147_PRO7311_POE.Models
{
    public enum ServiceRequestStatus { Pending, Approved, Rejected, Completed }

    [Table("ServiceRequests")]
    public class ServiceRequest
    {
        [Key]
        public int Id { get; set; }

        // Rubric: Linked to Contract, Description, Cost, Status
        [Required]
        public int ContractId { get; set; }
        public Contracts? Contract { get; set; }

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // USD amount entered by user
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostUSD { get; set; }

        // Auto-calculated ZAR cost (USD * live exchange rate)
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostZAR { get; set; }

        // Exchange rate used at time of creation (for audit)
        [Column(TypeName = "decimal(18,6)")]
        public decimal ExchangeRateUsed { get; set; }

        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    }
}

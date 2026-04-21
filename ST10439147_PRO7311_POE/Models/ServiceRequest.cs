using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10439147_PRO7311_POE.Models
{
    public enum ServiceRequestStatus { Pending, Approved, Rejected, Completed }

    [Table("ServiceRequests")]
    public class ServiceRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }
        public Contract? Contract { get; set; }

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // The amount entered by the user in their chosen source currency
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostUSD { get; set; }

        // The converted amount in the target currency
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostZAR { get; set; }

        // Exchange rate used at time of creation (for audit)
        [Column(TypeName = "decimal(18,6)")]
        public decimal ExchangeRateUsed { get; set; }

        // Currency codes chosen by the user (e.g. "EUR", "GBP", "ZAR")
        [MaxLength(10)]
        public string FromCurrency { get; set; } = "USD";

        [MaxLength(10)]
        public string ToCurrency { get; set; } = "ZAR";

        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ST10439147_PRO7311_POE.Models.Clients;

namespace ST10439147_PRO7311_POE.Models
{
    // Status machine: Draft → Active → Expired | OnHold
    public enum ContractStatus { Draft, Active, Expired, OnHold }

    public enum ServiceLvl { Standard, Premium, Express, Economy }

    [Table("Contract")]
    public class Contract
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        // ServiceLevel field on Contract
        [Required]
        public ServiceLvl ServiceLevel { get; set; }

        [Required]
        public int ClientId { get; set; }
        public Clients? Client { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // PDF "Signed Agreement" - path stored on disk, not as blob
        [MaxLength(500)]
        public string? SignedAgreementPath { get; set; }

        // Original filename for display in UI
        [MaxLength(300)]
        public string? SignedAgreementFileName { get; set; }

        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}

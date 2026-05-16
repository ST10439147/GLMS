namespace ST10439147_PRO7311_POE.Models
{
    public enum ContractStatus { Draft, Active, Expired, OnHold }

    public enum ServiceLvl { Standard, Premium, Express, Economy }

    public class Contract
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public ContractStatus Status { get; set; } = ContractStatus.Draft;
        public ServiceLvl ServiceLevel { get; set; }
        public int ClientId { get; set; }
        public Clients? Client { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? SignedAgreementPath { get; set; }
        public string? SignedAgreementFileName { get; set; }
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}

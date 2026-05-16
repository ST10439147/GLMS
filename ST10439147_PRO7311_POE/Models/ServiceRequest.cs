namespace ST10439147_PRO7311_POE.Models
{
    public enum ServiceRequestStatus { Pending, Approved, Rejected, Completed }

    public class ServiceRequest
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }
        public decimal ExchangeRateUsed { get; set; }
        public string FromCurrency { get; set; } = "USD";
        public string ToCurrency { get; set; } = "ZAR";
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        // Client-side navigation property — populated by the Blazor page, not from the API response.
        [System.Text.Json.Serialization.JsonIgnore]
        public Contract? Contract { get; set; }
    }
}

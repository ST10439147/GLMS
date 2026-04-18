using ST10439147_PRO7311_POE.Models;
namespace ST10439147_PRO7311_POE.DTOs
{
    public class ContractResponseDto
    {
        public Models.Contract? Contract { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; } = false;
    }

    public class ServiceRequestResponseDto
    {
        public ServiceRequest? ServiceRequest { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; } = false;
    }

    public class ContractFilterDto
    {
        public DateTime? StartFrom { get; set; }
        public DateTime? StartTo { get; set; }
        public ContractStatus? Status { get; set; }
    }

    public class CurrencyRateDto
    {
        public decimal Rate { get; set; }
        public decimal UsdAmount { get; set; }
        public decimal ZarAmount { get; set; }
    }
}

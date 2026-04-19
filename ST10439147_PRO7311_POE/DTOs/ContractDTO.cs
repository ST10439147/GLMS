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

        /// <summary>Source currency code, e.g. "USD"</summary>
        public string FromCurrency { get; set; } = "USD";

        /// <summary>Target currency code, e.g. "ZAR"</summary>
        public string ToCurrency { get; set; } = "ZAR";

        /// <summary>The input amount in FromCurrency</summary>
        public decimal FromAmount { get; set; }

        /// <summary>The converted amount in ToCurrency</summary>
        public decimal ToAmount { get; set; }
    }
}
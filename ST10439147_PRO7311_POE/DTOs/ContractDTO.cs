using ST10439147_PRO7311_POE.Models;
namespace ST10439147_PRO7311_POE.DTOs
{
    /// <summary>
    /// ContractResponseDto is a data transfer object used to encapsulate the response from contract-related operations.
    /// It contains the contract object, a message describing the result, and a status indicating success or failure.
    /// </summary>
    public class ContractResponseDto
    {
        public Models.Contract? Contract { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; } = false;
    }

    /// <summary>
    /// Represents the response data for a service request operation, including the request details, status, and an
    /// associated message.
    /// </summary>
    public class ServiceRequestResponseDto
    {
        public ServiceRequest? ServiceRequest { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; } = false;
    }
    /// <summary>
    /// Represents the filter criteria for searching contracts, including start date range and status.
    /// </summary>
    public class ContractFilterDto
    {
        public DateTime? StartFrom { get; set; }
        public DateTime? StartTo { get; set; }
        public ContractStatus? Status { get; set; }
    }

    public class CurrencyRateDto
    {
        public decimal Rate { get; set; }

        // Source currency code, e.g. "USD"
        public string FromCurrency { get; set; } = "USD";

        // Target currency code, e.g. "ZAR"
        public string ToCurrency { get; set; } = "ZAR";

        // The input amount in FromCurrency
        public decimal FromAmount { get; set; }

        // The converted amount in ToCurrency
        public decimal ToAmount { get; set; }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------
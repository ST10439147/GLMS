using ST10439147_PRO7311_POE_Back.Models;
namespace ST10439147_PRO7311_POE_Back.DTOs
{
    /// <summary>Response envelope for contract operations.</summary>
    public class ContractResponseDto
    {
        /// <summary>The contract affected by the operation, if any.</summary>
        public Models.Contract? Contract { get; set; }
        /// <summary>Human-readable result message.</summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>True when the operation succeeded.</summary>
        public bool Status { get; set; } = false;
    }

    /// <summary>Response envelope for service-request operations.</summary>
    public class ServiceRequestResponseDto
    {
        /// <summary>The service request affected by the operation, if any.</summary>
        public ServiceRequest? ServiceRequest { get; set; }
        /// <summary>Human-readable result message.</summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>True when the operation succeeded.</summary>
        public bool Status { get; set; } = false;
    }

    /// <summary>Query-filter parameters for listing contracts.</summary>
    public class ContractFilterDto
    {
        /// <summary>Return contracts whose start date is on or after this date.</summary>
        public DateTime? StartFrom { get; set; }
        /// <summary>Return contracts whose start date is on or before this date.</summary>
        public DateTime? StartTo { get; set; }
        /// <summary>Return contracts with this status only.</summary>
        public ContractStatus? Status { get; set; }
    }

    /// <summary>Request body for PATCH /api/contracts/{id}/status.</summary>
    public class StatusUpdateDto
    {
        /// <summary>The new contract status to apply (Draft, Active, Expired, OnHold).</summary>
        public ContractStatus Status { get; set; }
    }

    /// <summary>Request body for PATCH /api/contracts/requests/{requestId}/status.</summary>
    public class RequestStatusUpdateDto
    {
        /// <summary>The new service-request status to apply (Pending, Approved, Rejected, Completed).</summary>
        public ServiceRequestStatus Status { get; set; }
    }

    /// <summary>Currency exchange rate and conversion result.</summary>
    public class CurrencyRateDto
    {
        /// <summary>Exchange rate (1 unit of FromCurrency expressed in ToCurrency).</summary>
        public decimal Rate { get; set; }
        /// <summary>Source currency code, e.g. "USD".</summary>
        public string FromCurrency { get; set; } = "USD";
        /// <summary>Target currency code, e.g. "ZAR".</summary>
        public string ToCurrency { get; set; } = "ZAR";
        /// <summary>The input amount in FromCurrency.</summary>
        public decimal FromAmount { get; set; }
        /// <summary>The converted amount in ToCurrency.</summary>
        public decimal ToAmount { get; set; }
    }
}

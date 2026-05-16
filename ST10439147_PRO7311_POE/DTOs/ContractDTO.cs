using ST10439147_PRO7311_POE.Models;

namespace ST10439147_PRO7311_POE.DTOs
{
    public class ContractResponseDto
    {
        public Contract? Contract { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; } = false;
    }

    public class CurrencyRateDto
    {
        public decimal Rate { get; set; }
        public string FromCurrency { get; set; } = "USD";
        public string ToCurrency { get; set; } = "ZAR";
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }
    }
}

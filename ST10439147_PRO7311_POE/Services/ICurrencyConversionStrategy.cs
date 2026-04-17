namespace ST10439147_PRO7311_POE.Services
{
    // ─── Strategy Pattern ─────────────────────────────────────────────────────
    // ICurrencyConversionStrategy allows switching FX providers via one
    // Program.cs line change. FinancialService never changes.

    public interface ICurrencyConversionStrategy
    {
        /// <summary>Convert amount from USD to ZAR using a live exchange rate.</summary>
        Task<decimal> ConvertUsdToZar(decimal usdAmount);

        /// <summary>Get the current USD→ZAR rate (for display on the UI).</summary>
        Task<decimal> GetUsdToZarRate();
    }

    public class CurrencyConversionException : Exception
    {
        public CurrencyConversionException(string message, Exception? inner = null)
            : base(message, inner) { }
    }

    // ── OpenExchangeRates strategy ────────────────────────────────────────────
    public class OpenExchangeStrategy : ICurrencyConversionStrategy
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<OpenExchangeStrategy> _logger;

        public OpenExchangeStrategy(
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ILogger<OpenExchangeStrategy> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        public async Task<decimal> GetUsdToZarRate()
        {
            try
            {
                var appId = _config["FxProviders:OpenExchangeRates:AppId"] ?? "";
                var client = _httpClientFactory.CreateClient("FxClient");

                // Free plan: base is always USD
                var url = string.IsNullOrWhiteSpace(appId)
                    ? "https://open.er-api.com/v6/latest/USD"          // fallback free API (no key)
                    : $"https://openexchangerates.org/api/latest.json?app_id={appId}&symbols=ZAR";

                var response = await client.GetFromJsonAsync<ExchangeRateResponse>(url);

                if (response?.Rates == null)
                    throw new CurrencyConversionException("Empty response from exchange rate API.");

                if (!response.Rates.TryGetValue("ZAR", out var zarRate))
                    throw new CurrencyConversionException("ZAR rate not found in response.");

                return zarRate;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Exchange rate API HTTP error");
                throw new CurrencyConversionException("Exchange rate request failed.", ex);
            }
        }

        public async Task<decimal> ConvertUsdToZar(decimal usdAmount)
        {
            var rate = await GetUsdToZarRate();
            return Math.Round(usdAmount * rate, 2);
        }

        // Works with both open.er-api.com and openexchangerates.org response shapes
        private record ExchangeRateResponse(Dictionary<string, decimal>? Rates);
    }
}

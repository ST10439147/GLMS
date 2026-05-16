namespace ST10439147_PRO7311_POE_Back.Services
{
    // Strategy pattern for currency conversion.
    // This allows us to easily switch between different FX providers by implementing the same interface.
    // ICurrencyConversionStrategy allows switching FX providers via one
    // Program.cs line change. FinancialService never changes.

    public interface ICurrencyConversionStrategy
    {
        //Convert an amount from one currency to another using a live rate.
        Task<decimal> Convert(decimal amount, string fromCurrency, string toCurrency);

        // Get the live exchange rate between two currencies.
        Task<decimal> GetRate(string fromCurrency, string toCurrency);

        // Return all available currency codes supported by the provider.
        Task<List<string>> GetAvailableCurrencies();
    }

    public class CurrencyConversionException : Exception
    {
        public CurrencyConversionException(string message, Exception? inner = null)
            : base(message, inner) { }
    }

    // OpenExchangeRates.org is a popular FX API with a free tier that doesn't require an API key. It provides exchange rates relative to USD, so we convert via USD as the base currency.
    public class OpenExchangeStrategy : ICurrencyConversionStrategy
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<OpenExchangeStrategy> _logger;

        // In-memory cache so we don't hammer the API on every keystroke
        private Dictionary<string, decimal>? _cachedRates;   // all rates relative to USD
        private DateTime _cacheExpiry = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public OpenExchangeStrategy(
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ILogger<OpenExchangeStrategy> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        // Fetches exchange rates from the API, with caching. Rates are returned as a dictionary where keys are currency codes and values are rates relative to USD.
        // If the cache is still valid, it returns the cached rates. Otherwise, it makes an HTTP request to fetch fresh rates, updates the cache, and returns the new rates.
        private async Task<Dictionary<string, decimal>> FetchRates()
        {
            if (_cachedRates != null && DateTime.UtcNow < _cacheExpiry)
                return _cachedRates;

            try
            {
                var appId = _config["FxProviders:OpenExchangeRates:AppId"] ?? "";
                var client = _httpClientFactory.CreateClient("FxClient");

                var url = string.IsNullOrWhiteSpace(appId)
                    ? "https://open.er-api.com/v6/latest/USD"          // free, no key
                    : $"https://openexchangerates.org/api/latest.json?app_id={appId}";

                var opts = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var response = await client.GetFromJsonAsync<ExchangeRateResponse>(url, opts);

                if (response?.Rates == null || response.Rates.Count == 0)
                    throw new CurrencyConversionException("Empty response from exchange rate API.");

                _cachedRates = response.Rates;
                _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
                return _cachedRates;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Exchange rate API HTTP error");
                throw new CurrencyConversionException("Exchange rate request failed.", ex);
            }
        }

        // Get the exchange rate from 'fromCurrency' to 'toCurrency' using USD as the base.
        public async Task<decimal> GetRate(string fromCurrency, string toCurrency)
        {
            var from = fromCurrency.ToUpperInvariant();
            var to = toCurrency.ToUpperInvariant();

            var rates = await FetchRates();   // all rates are relative to USD

            if (!rates.TryGetValue(from, out var fromRate))
                throw new CurrencyConversionException($"Currency '{from}' not found.");
            if (!rates.TryGetValue(to, out var toRate))
                throw new CurrencyConversionException($"Currency '{to}' not found.");

            // Convert via USD as the base:  1 from = (1/fromRate) USD = (toRate/fromRate) to
            return Math.Round(toRate / fromRate, 6);
        }

        // Convert an amount from 'fromCurrency' to 'toCurrency' using the live exchange rate.
        public async Task<decimal> Convert(decimal amount, string fromCurrency, string toCurrency)
        {
            var rate = await GetRate(fromCurrency, toCurrency);
            return Math.Round(amount * rate, 2);
        }

        // Return a sorted list of available currency codes supported by the provider.
        public async Task<List<string>> GetAvailableCurrencies()
        {
            var rates = await FetchRates();
            return rates.Keys.OrderBy(k => k).ToList();
        }

        private record ExchangeRateResponse(Dictionary<string, decimal>? Rates);
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------
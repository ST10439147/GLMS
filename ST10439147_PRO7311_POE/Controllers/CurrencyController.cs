using Microsoft.AspNetCore.Mvc;
using ST10439147_PRO7311_POE.DTOs;
using ST10439147_PRO7311_POE.Services;

// This controller provides endpoints for currency conversion using the ICurrencyConversionStrategy service.
// It includes endpoints to get the list of supported currencies, retrieve exchange rates, and convert amounts between currencies.
// The controller handles exceptions from the currency conversion service and returns appropriate HTTP status codes and messages.
[ApiController]
[Route("api/v1/[controller]")]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyConversionStrategy _fxStrategy;

    public CurrencyController(ICurrencyConversionStrategy fxStrategy)
    {
        _fxStrategy = fxStrategy;
    }

    // GET api/v1/currency/currencies
    // Returns the list of all supported currency codes (e.g. ["AUD","EUR","GBP","USD","ZAR",...])
    // If the list cannot be retrieved (e.g. API failure), returns a 503 status with an error message
    [HttpGet("currencies")]
    public async Task<ActionResult<List<string>>> GetCurrencies()
    {
        try
        {
            var currencies = await _fxStrategy.GetAvailableCurrencies();
            return Ok(currencies);
        }
        catch (CurrencyConversionException ex)
        {
            return StatusCode(503, new { Message = ex.Message });
        }
    }

    // GET api/v1/currency/rate?from=USD&to=ZAR
    // Returns the current exchange rate between any two currencies
    // If the rate cannot be retrieved (e.g. API failure), returns a 503 status with an error message
    // Example response: { "fromCurrency": "USD", "toCurrency": "ZAR", "rate": 18.5 }
    [HttpGet("rate")]
    public async Task<ActionResult<CurrencyRateDto>> GetRate(
        [FromQuery] string from = "USD",
        [FromQuery] string to = "ZAR")
    {
        try
        {
            var rate = await _fxStrategy.GetRate(from, to);
            return Ok(new CurrencyRateDto
            {
                Rate = rate,
                FromCurrency = from.ToUpperInvariant(),
                ToCurrency = to.ToUpperInvariant(),
                FromAmount = 1,
                ToAmount = rate
            });
        }
        catch (CurrencyConversionException ex)
        {
            return StatusCode(503, new { Message = ex.Message });
        }
    }

    // GET api/v1/currency/convert?amount=100&from=USD&to=ZAR
    // Converts any amount between any two currencies (called live as user types)
    // Returns the converted amount along with the exchange rate used for the conversion
    // If the conversion cannot be performed (e.g. API failure), returns a 503 status with an error message
    [HttpGet("convert")]
    public async Task<ActionResult<CurrencyRateDto>> Convert(
        [FromQuery] decimal amount,
        [FromQuery] string from = "USD",
        [FromQuery] string to = "ZAR")
    {
        try
        {
            var rate = await _fxStrategy.GetRate(from, to);
            var converted = Math.Round(amount * rate, 2);
            return Ok(new CurrencyRateDto
            {
                Rate = rate,
                FromCurrency = from.ToUpperInvariant(),
                ToCurrency = to.ToUpperInvariant(),
                FromAmount = amount,
                ToAmount = converted
            });
        }
        catch (CurrencyConversionException ex)
        {
            return StatusCode(503, new { Message = ex.Message });
        }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------
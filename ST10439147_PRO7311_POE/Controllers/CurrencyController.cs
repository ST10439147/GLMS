using Microsoft.AspNetCore.Mvc;
using ST10439147_PRO7311_POE.DTOs;
using ST10439147_PRO7311_POE.Services;

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
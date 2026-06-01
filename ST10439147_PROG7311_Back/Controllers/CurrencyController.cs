using Microsoft.AspNetCore.Mvc;
using ST10439147_PRO7311_POE_Back.DTOs;
using ST10439147_PRO7311_POE_Back.Services;

namespace ST10439147_PRO7311_POE_Back.Controllers
{
    /// <summary>
    /// Provides live currency exchange rates and conversion using the OpenExchangeRates API.
    /// Rates are cached for 10 minutes to avoid excessive external API calls.
    /// </summary>
    [ApiController]
    [Route("api/currency")]
    [Produces("application/json")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyConversionStrategy _fxStrategy;

        public CurrencyController(ICurrencyConversionStrategy fxStrategy)
        {
            _fxStrategy = fxStrategy;
        }

        /// <summary>
        /// Returns the list of all supported currency codes (e.g. "AUD", "EUR", "USD", "ZAR").
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
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

        /// <summary>
        /// Returns the current exchange rate between two currencies.
        /// </summary>
        /// <param name="from">Source currency code (default: USD).</param>
        /// <param name="to">Target currency code (default: ZAR).</param>
        [HttpGet("rate")]
        [ProducesResponseType(typeof(CurrencyRateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
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

        /// <summary>
        /// Converts an amount from one currency to another.
        /// </summary>
        /// <param name="amount">The amount to convert.</param>
        /// <param name="from">Source currency code (default: USD).</param>
        /// <param name="to">Target currency code (default: ZAR).</param>
        [HttpGet("convert")]
        [ProducesResponseType(typeof(CurrencyRateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
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
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------
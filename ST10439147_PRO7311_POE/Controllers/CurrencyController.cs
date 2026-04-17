using GLMS.Services;
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

    // GET api/v1/currency/rate
    // Returns current USD→ZAR rate (called by JS on the ServiceRequest page)
    [HttpGet("rate")]
    public async Task<ActionResult<CurrencyRateDto>> GetRate()
    {
        try
        {
            var rate = await _fxStrategy.GetUsdToZarRate();
            return Ok(new CurrencyRateDto { Rate = rate, UsdAmount = 1, ZarAmount = rate });
        }
        catch (CurrencyConversionException ex)
        {
            return StatusCode(503, new { Message = ex.Message });
        }
    }

    // GET api/v1/currency/convert?usd=100
    // Auto-calculates ZAR from a USD amount (called live as user types)
    [HttpGet("convert")]
    public async Task<ActionResult<CurrencyRateDto>> Convert([FromQuery] decimal usd)
    {
        try
        {
            var rate = await _fxStrategy.GetUsdToZarRate();
            var zar = Math.Round(usd * rate, 2);
            return Ok(new CurrencyRateDto { Rate = rate, UsdAmount = usd, ZarAmount = zar });
        }
        catch (CurrencyConversionException ex)
        {
            return StatusCode(503, new { Message = ex.Message });
        }
    }
}

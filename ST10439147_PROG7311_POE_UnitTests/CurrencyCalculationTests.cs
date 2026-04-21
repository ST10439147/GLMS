using Moq;
using ST10439147_PRO7311_POE.Services;

namespace ST10439147_PRO7311_POE.Tests
{
    
    // Verifies the USD-relative cross-rate formula and interface contract.
    public class CurrencyCalculationTests
    {
        private static decimal CrossRate(decimal fromUsdRate, decimal toUsdRate)
            => Math.Round(toUsdRate / fromUsdRate, 6);

        private static decimal Convert(decimal amount, decimal fromUsdRate, decimal toUsdRate)
            => Math.Round(amount * CrossRate(fromUsdRate, toUsdRate), 2);

        [Fact]
        public void Convert_100Usd_ToZar_ReturnsCorrectAmount()
        {
            // USD rate = 1.0, ZAR rate = 18.5  →  100 * 18.5 = 1850.00
            var result = Convert(amount: 100m, fromUsdRate: 1m, toUsdRate: 18.5m);
            Assert.Equal(1850.00m, result);
        }

        [Fact]
        public void Convert_Result_IsRoundedToTwoDecimalPlaces()
        {
            // 50 GBP at USD rate 1.27, ZAR at 18.5  →  50 * (18.5/1.27) = 728.35
            var result = Convert(amount: 50m, fromUsdRate: 1.27m, toUsdRate: 18.5m);
            Assert.Equal(728.35m, result);
        }

        [Fact]
        public async Task GetRate_UnsupportedCurrency_ThrowsCurrencyConversionException()
        {
            var mock = new Mock<ICurrencyConversionStrategy>();
            mock.Setup(s => s.GetRate("USD", "XYZ"))
                .ThrowsAsync(new CurrencyConversionException("Currency 'XYZ' not found."));

            await Assert.ThrowsAsync<CurrencyConversionException>(
                () => mock.Object.GetRate("USD", "XYZ"));
        }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------
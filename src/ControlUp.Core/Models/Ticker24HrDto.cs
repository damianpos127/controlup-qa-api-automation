using System.Text.Json.Serialization;

namespace ControlUp.Core.Models;

/// <summary>
/// DTO representing 24-hour ticker statistics for a trading symbol
/// </summary>
public class Ticker24HrDto
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("priceChangePercent")]
    public string PriceChangePercent { get; set; } = "0";

    [JsonPropertyName("lastPrice")]
    public string LastPrice { get; set; } = "0";

    [JsonPropertyName("volume")]
    public string Volume { get; set; } = "0";

    [JsonPropertyName("highPrice")]
    public string HighPrice { get; set; } = "0";

    [JsonPropertyName("lowPrice")]
    public string LowPrice { get; set; } = "0";

    /// <summary>
    /// Parses PriceChangePercent as decimal, returns 0 if parsing fails
    /// </summary>
    public decimal GetPriceChangePercentAsDecimal()
    {
        if (decimal.TryParse(PriceChangePercent, out var result))
        {
            return result;
        }
        return 0;
    }
}

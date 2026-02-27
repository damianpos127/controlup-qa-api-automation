using System.Text.Json.Serialization;

namespace ControlUp.Core.Models;

/// <summary>
/// DTO representing average price for a trading symbol
/// </summary>
public class AvgPriceDto
{
    [JsonPropertyName("mins")]
    public int Mins { get; set; }

    [JsonPropertyName("price")]
    public string Price { get; set; } = "0";

    /// <summary>
    /// Parses Price as decimal, returns 0 if parsing fails
    /// </summary>
    public decimal GetPriceAsDecimal()
    {
        if (decimal.TryParse(Price, out var result))
        {
            return result;
        }
        return 0;
    }
}

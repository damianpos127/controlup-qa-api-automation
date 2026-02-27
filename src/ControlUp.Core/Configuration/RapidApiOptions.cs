namespace ControlUp.Core.Configuration;

/// <summary>
/// Configuration options for RapidAPI Binance endpoint
/// </summary>
public class RapidApiOptions
{
    public const string SectionName = "RapidApi";

    /// <summary>
    /// Base URL for the RapidAPI Binance endpoint
    /// </summary>
    public string BaseUrl { get; set; } = "https://binance43.p.rapidapi.com";

    /// <summary>
    /// RapidAPI key (X-RapidAPI-Key header)
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// RapidAPI host (X-RapidAPI-Host header)
    /// </summary>
    public string Host { get; set; } = "binance43.p.rapidapi.com";
}

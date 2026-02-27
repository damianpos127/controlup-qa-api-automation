using ControlUp.Core.Models;

namespace ControlUp.Core.Clients;

/// <summary>
/// Interface for Binance API client operations
/// </summary>
public interface IBinanceApiClient
{
    /// <summary>
    /// Gets 24-hour ticker statistics for all symbols
    /// </summary>
    Task<List<Ticker24HrDto>> GetTicker24HrAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets average price for a specific symbol
    /// </summary>
    Task<AvgPriceDto> GetAvgPriceAsync(string symbol, CancellationToken cancellationToken = default);
}

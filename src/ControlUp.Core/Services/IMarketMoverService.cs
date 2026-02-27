using ControlUp.Core.Models;

namespace ControlUp.Core.Services;

/// <summary>
/// Service for identifying and reporting top market movers
/// </summary>
public interface IMarketMoverService
{
    /// <summary>
    /// Gets the top N market movers by 24-hour price change percent and their average prices
    /// </summary>
    Task<MarketMoverReport> GetTopMoversReportAsync(int topCount = 3, CancellationToken cancellationToken = default);
}

using ControlUp.Core.Clients;
using ControlUp.Core.Models;
using Microsoft.Extensions.Logging;

namespace ControlUp.Core.Services;

/// <summary>
/// Service implementation for identifying top market movers
/// </summary>
public class MarketMoverService : IMarketMoverService
{
    private readonly IBinanceApiClient _apiClient;
    private readonly ILogger<MarketMoverService> _logger;

    public MarketMoverService(
        IBinanceApiClient apiClient,
        ILogger<MarketMoverService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<MarketMoverReport> GetTopMoversReportAsync(int topCount = 3, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("Starting market mover analysis for top {TopCount} symbols", topCount);

        // Step 1: Get all 24-hour ticker data
        var tickers = await _apiClient.GetTicker24HrAsync(cancellationToken);

        if (tickers == null || tickers.Count == 0)
        {
            _logger.LogWarning("No ticker data received from API");
            var completionTime = DateTime.UtcNow;
            return new MarketMoverReport
            {
                TestExecutionTime = DateTime.MinValue, // Will be set by test
                GeneratedAt = completionTime,
                Runtime = completionTime - startTime,
                TopMovers = new List<MarketMoverItem>()
            };
        }

        // Step 2: Filter and sort by priceChangePercent (descending)
        _logger.LogInformation("Total tickers received: {Count}", tickers.Count);
        
        // Filter out invalid entries
        var validTickers = tickers
            .Where(t => !string.IsNullOrWhiteSpace(t.Symbol) && !string.IsNullOrWhiteSpace(t.PriceChangePercent))
            .ToList();
        
        _logger.LogInformation("Valid tickers after filtering: {Count}", validTickers.Count);
        
        // Sort by priceChangePercent descending and take top N
        var sortedTickers = validTickers
            .OrderByDescending(t => t.GetPriceChangePercentAsDecimal())
            .ToList();
        
        // Log top 10 for verification
        _logger.LogInformation("Top 10 symbols by priceChangePercent:");
        for (int i = 0; i < Math.Min(10, sortedTickers.Count); i++)
        {
            var ticker = sortedTickers[i];
            var percent = ticker.GetPriceChangePercentAsDecimal();
            _logger.LogInformation("  {Rank}. {Symbol}: {Percent}% (raw: '{Raw}')", 
                i + 1, ticker.Symbol, percent, ticker.PriceChangePercent);
        }
        
        var topMovers = sortedTickers.Take(topCount).ToList();

        if (topMovers.Count == 0)
        {
            _logger.LogWarning("No valid tickers found after filtering");
            var completionTime = DateTime.UtcNow;
            return new MarketMoverReport
            {
                TestExecutionTime = DateTime.MinValue, // Will be set by test
                GeneratedAt = completionTime,
                Runtime = completionTime - startTime,
                TopMovers = new List<MarketMoverItem>()
            };
        }

        _logger.LogInformation("Selected top {Count} movers for report: {Symbols}",
            topMovers.Count,
            string.Join(", ", topMovers.Select(t => $"{t.Symbol}({t.GetPriceChangePercentAsDecimal():F2}%)")));

        // Step 3: Get average prices for each top mover
        var reportItems = new List<MarketMoverItem>();

        for (int i = 0; i < topMovers.Count; i++)
        {
            var ticker = topMovers[i];
            try
            {
                var avgPrice = await _apiClient.GetAvgPriceAsync(ticker.Symbol, cancellationToken);

                reportItems.Add(new MarketMoverItem
                {
                    Rank = i + 1,
                    Symbol = ticker.Symbol,
                    PriceChangePercent24Hr = ticker.GetPriceChangePercentAsDecimal(),
                    AveragePrice = avgPrice.GetPriceAsDecimal()
                });

                _logger.LogInformation("Processed {Rank}. {Symbol}: {Change}% change, {Price} avg price",
                    i + 1, ticker.Symbol, ticker.GetPriceChangePercentAsDecimal(), avgPrice.GetPriceAsDecimal());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get average price for symbol: {Symbol}", ticker.Symbol);
                // Continue with other symbols even if one fails
                reportItems.Add(new MarketMoverItem
                {
                    Rank = i + 1,
                    Symbol = ticker.Symbol,
                    PriceChangePercent24Hr = ticker.GetPriceChangePercentAsDecimal(),
                    AveragePrice = 0
                });
            }
        }

        var endTime = DateTime.UtcNow;
        var runtime = endTime - startTime;
        
        _logger.LogInformation("Market mover analysis completed in {Runtime}ms", runtime.TotalMilliseconds);
        
        return new MarketMoverReport
        {
            TestExecutionTime = DateTime.MinValue, // Will be set by test
            GeneratedAt = endTime,
            Runtime = runtime,
            TopMovers = reportItems
        };
    }
}

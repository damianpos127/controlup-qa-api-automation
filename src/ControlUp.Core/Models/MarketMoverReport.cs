namespace ControlUp.Core.Models;

/// <summary>
/// Report model containing top market movers with their average prices
/// </summary>
public class MarketMoverReport
{
    /// <summary>
    /// When the test execution started
    /// </summary>
    public DateTime TestExecutionTime { get; set; }

    /// <summary>
    /// When the report was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Total runtime duration for generating the report
    /// </summary>
    public TimeSpan Runtime { get; set; }

    public List<MarketMoverItem> TopMovers { get; set; } = new();
}

/// <summary>
/// Individual market mover item in the report
/// </summary>
public class MarketMoverItem
{
    public int Rank { get; set; }

    public string Symbol { get; set; } = string.Empty;

    public decimal PriceChangePercent24Hr { get; set; }

    public decimal AveragePrice { get; set; }
}

using ControlUp.Core.Models;

namespace ControlUp.Core.Reporting;

/// <summary>
/// Service for generating reports in various formats
/// </summary>
public interface IReportGenerator
{
    /// <summary>
    /// Generates reports in JSON and Markdown formats and saves them to the artifacts directory
    /// </summary>
    Task GenerateReportsAsync(MarketMoverReport report, string artifactsDirectory = "artifacts", CancellationToken cancellationToken = default);
}

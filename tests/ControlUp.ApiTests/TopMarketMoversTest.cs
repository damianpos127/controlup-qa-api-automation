using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ControlUp.Core.Clients;
using ControlUp.Core.Configuration;
using ControlUp.Core.Models;
using ControlUp.Core.Reporting;
using ControlUp.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace ControlUp.ApiTests;

/// <summary>
/// Integration tests for the market mover automation framework
/// </summary>
public class TopMarketMoversTest : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMarketMoverService _marketMoverService;
    private readonly IReportGenerator _reportGenerator;
    private readonly ITestOutputHelper _output;

    public TopMarketMoversTest(ITestOutputHelper output)
    {
        _output = output;

        // Build configuration
        // Get the directory where the test assembly is located
        var testDirectory = Path.GetDirectoryName(typeof(TopMarketMoversTest).Assembly.Location) 
            ?? Directory.GetCurrentDirectory();
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(testDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Setup dependency injection
        var services = new ServiceCollection();

        // Configure RapidAPI options
        services.Configure<RapidApiOptions>(configuration.GetSection(RapidApiOptions.SectionName));

        // Register HttpClient for API client
        services.AddHttpClient<IBinanceApiClient, BinanceApiClient>();

        // Register services
        services.AddScoped<IMarketMoverService, MarketMoverService>();
        services.AddScoped<IReportGenerator, ReportGenerator>();

        // Register logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        _serviceProvider = services.BuildServiceProvider();
        _marketMoverService = _serviceProvider.GetRequiredService<IMarketMoverService>();
        _reportGenerator = _serviceProvider.GetRequiredService<IReportGenerator>();
    }

    [Fact]
    public async Task GetTop3MarketMovers_ShouldReturnValidReport()
    {
        // Arrange
        const int topCount = 3;
        var testExecutionTime = DateTime.UtcNow;

        // Act
        var report = await _marketMoverService.GetTopMoversReportAsync(topCount);
        report.TestExecutionTime = testExecutionTime;

        // Assert - Structure and sanity checks (not exact values, as prices change)
        Assert.NotNull(report);
        Assert.True(report.GeneratedAt > DateTime.MinValue);
        Assert.NotNull(report.TopMovers);
        Assert.True(report.TopMovers.Count > 0, "Report should contain at least one market mover");
        Assert.True(report.TopMovers.Count <= topCount, $"Report should contain at most {topCount} movers");

        // Verify all items have valid data
        foreach (var mover in report.TopMovers)
        {
            Assert.False(string.IsNullOrWhiteSpace(mover.Symbol), "Symbol should not be empty");
            Assert.True(mover.Rank > 0, "Rank should be positive");
            Assert.True(mover.Rank <= topCount, $"Rank should not exceed {topCount}");

            // Note: We don't assert exact price values as they change constantly
            // We only verify the structure is valid
        }

        // Verify symbols are distinct
        var symbols = report.TopMovers.Select(m => m.Symbol).ToList();
        Assert.Equal(symbols.Count, symbols.Distinct().Count());

        // Verify ordering (should be sorted by price change percent descending)
        for (int i = 0; i < report.TopMovers.Count - 1; i++)
        {
            var current = report.TopMovers[i];
            var next = report.TopMovers[i + 1];
            Assert.True(
                current.PriceChangePercent24Hr >= next.PriceChangePercent24Hr,
                $"Movers should be sorted by price change percent descending. {current.Symbol} ({current.PriceChangePercent24Hr}%) should be >= {next.Symbol} ({next.PriceChangePercent24Hr}%)");
        }

        // Output results for visibility
        _output.WriteLine($"\n=== Top {topCount} Market Movers Report ===");
        _output.WriteLine($"Generated At: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC\n");
        _output.WriteLine("Rank | Symbol      | 24h Change % | Average Price");
        _output.WriteLine("-----|-------------|--------------|---------------");

        foreach (var mover in report.TopMovers)
        {
            _output.WriteLine($"{mover.Rank,4} | {mover.Symbol,-11} | {mover.PriceChangePercent24Hr,12:F2}% | {mover.AveragePrice:F8}");
        }
    }

    [Fact]
    public async Task GetTop3MarketMovers_ShouldGenerateReports()
    {
        // Arrange
        const int topCount = 3;
        var testExecutionTime = DateTime.UtcNow;
        
        // Get the solution root directory - works in both local and CI environments
        // In CI, use GITHUB_WORKSPACE environment variable (set by GitHub Actions)
        // Locally, navigate from test output directory to solution root
        string artifactsDir;
        var workspace = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE");
        
        if (!string.IsNullOrEmpty(workspace))
        {
            // CI environment - use workspace root
            artifactsDir = Path.Combine(workspace, "artifacts");
        }
        else
        {
            // Local environment - navigate from test output to solution root
            var currentDir = Directory.GetCurrentDirectory();
            if (currentDir.Contains("bin") || currentDir.Contains("obj"))
            {
                var testAssemblyDir = Path.GetDirectoryName(typeof(TopMarketMoversTest).Assembly.Location) ?? currentDir;
                var solutionRoot = Path.GetFullPath(Path.Combine(testAssemblyDir, "..", "..", "..", "..", ".."));
                artifactsDir = Path.Combine(solutionRoot, "artifacts");
            }
            else
            {
                // Already at solution root
                artifactsDir = Path.Combine(currentDir, "artifacts");
            }
        }

        // Act
        var report = await _marketMoverService.GetTopMoversReportAsync(topCount);
        report.TestExecutionTime = testExecutionTime;
        await _reportGenerator.GenerateReportsAsync(report, artifactsDir);

        // Assert - Verify files were created
        var jsonPath = Path.Combine(artifactsDir, "report.json");
        var mdPath = Path.Combine(artifactsDir, "report.md");

        Assert.True(File.Exists(jsonPath), $"JSON report should exist at {jsonPath}");
        Assert.True(File.Exists(mdPath), $"Markdown report should exist at {mdPath}");

        // Verify JSON content is valid
        var jsonContent = await File.ReadAllTextAsync(jsonPath);
        Assert.False(string.IsNullOrWhiteSpace(jsonContent), "JSON report should not be empty");

        // Verify Markdown content is valid
        var mdContent = await File.ReadAllTextAsync(mdPath);
        Assert.False(string.IsNullOrWhiteSpace(mdContent), "Markdown report should not be empty");
        Assert.Contains("Top Market Movers Report", mdContent);
        Assert.Contains("Rank", mdContent);
        Assert.Contains("Symbol", mdContent);

        _output.WriteLine($"\nReports generated successfully:");
        _output.WriteLine($"  JSON: {jsonPath}");
        _output.WriteLine($"  Markdown: {mdPath}");
    }

    [Fact]
    public async Task GetTop3MarketMovers_AllAveragePricesShouldBePositive()
    {
        // Arrange
        const int topCount = 3;
        var testExecutionTime = DateTime.UtcNow;

        // Act
        var report = await _marketMoverService.GetTopMoversReportAsync(topCount);
        report.TestExecutionTime = testExecutionTime;

        // Assert - All average prices should be positive (market data sanity check)
        foreach (var mover in report.TopMovers)
        {
            Assert.True(
                mover.AveragePrice > 0,
                $"Average price for {mover.Symbol} should be positive, but was {mover.AveragePrice}");
        }
    }

    public void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

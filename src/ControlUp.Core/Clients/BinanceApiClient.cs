using System.Text.Json;
using ControlUp.Core.Configuration;
using ControlUp.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ControlUp.Core.Clients;

/// <summary>
/// Implementation of Binance API client using RapidAPI
/// </summary>
public class BinanceApiClient : IBinanceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly RapidApiOptions _options;
    private readonly ILogger<BinanceApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public BinanceApiClient(
        HttpClient httpClient,
        IOptions<RapidApiOptions> options,
        ILogger<BinanceApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        // Validate configuration
        if (string.IsNullOrWhiteSpace(_options.Key))
        {
            throw new InvalidOperationException(
                "RapidAPI Key is not configured. Please set RapidApi:Key in appsettings.json or via environment variable RapidApi__Key");
        }

        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            throw new InvalidOperationException(
                "RapidAPI Host is not configured. Please set RapidApi:Host in appsettings.json or via environment variable RapidApi__Host");
        }

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", _options.Key);
        _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", _options.Host);
        // Prevent HTTP caching to ensure fresh data
        _httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
        {
            NoCache = true,
            NoStore = true,
            MustRevalidate = true
        };
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        // Configure JSON serialization
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<Ticker24HrDto>> GetTicker24HrAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var requestTime = DateTime.UtcNow;
            _logger.LogInformation("Fetching 24-hour ticker data from Binance API at {RequestTime}", requestTime);

            // NOTE: The Binance 24hr ticker endpoint does not accept extra query parameters,
            // so we must call it with the exact path. Rely on Cache-Control headers instead
            // to discourage intermediaries from caching the response.
            var response = await _httpClient.GetAsync("/ticker/24hr", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var errorMessage = GetErrorMessage(response.StatusCode, errorContent, "ticker/24hr");
                _logger.LogError("API request failed with status {StatusCode}: {ErrorMessage}", response.StatusCode, errorMessage);
                throw new HttpRequestException(errorMessage, null, response.StatusCode);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tickers = JsonSerializer.Deserialize<List<Ticker24HrDto>>(content, _jsonOptions);

            if (tickers == null)
            {
                _logger.LogWarning("API returned null ticker data");
                return new List<Ticker24HrDto>();
            }

            _logger.LogInformation("Successfully fetched {Count} tickers at {ResponseTime}. Top 3 by change: {TopSymbols}",
                tickers.Count,
                DateTime.UtcNow,
                string.Join(", ", tickers
                    .OrderByDescending(t => t.GetPriceChangePercentAsDecimal())
                    .Take(3)
                    .Select(t => $"{t.Symbol}({t.GetPriceChangePercentAsDecimal():F2}%)")));
            return tickers;
        }
        catch (HttpRequestException)
        {
            // Re-throw with enhanced error message
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error while parsing ticker data");
            throw;
        }
    }

    public async Task<AvgPriceDto> GetAvgPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol cannot be null or empty", nameof(symbol));
        }

        try
        {
            _logger.LogInformation("Fetching average price for symbol: {Symbol}", symbol);

            var response = await _httpClient.GetAsync($"/avgPrice?symbol={Uri.EscapeDataString(symbol)}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var errorMessage = GetErrorMessage(response.StatusCode, errorContent, $"avgPrice?symbol={symbol}");
                _logger.LogError("API request failed with status {StatusCode}: {ErrorMessage}", response.StatusCode, errorMessage);
                throw new HttpRequestException(errorMessage, null, response.StatusCode);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var avgPrice = JsonSerializer.Deserialize<AvgPriceDto>(content, _jsonOptions);

            if (avgPrice == null)
            {
                _logger.LogWarning("API returned null average price data for symbol: {Symbol}", symbol);
                return new AvgPriceDto();
            }

            _logger.LogInformation("Successfully fetched average price for {Symbol}: {Price}", symbol, avgPrice.Price);
            return avgPrice;
        }
        catch (HttpRequestException)
        {
            // Re-throw with enhanced error message
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error while parsing average price for symbol: {Symbol}", symbol);
            throw;
        }
    }

    private static string GetErrorMessage(System.Net.HttpStatusCode statusCode, string? errorContent, string endpoint)
    {
        return statusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => 
                $"401 Unauthorized: Invalid or missing RapidAPI key. Please verify your RapidApi:Key configuration. Endpoint: {endpoint}",
            System.Net.HttpStatusCode.TooManyRequests => 
                $"429 Too Many Requests: Rate limit exceeded. Please wait before retrying. Endpoint: {endpoint}",
            System.Net.HttpStatusCode.NotFound => 
                $"404 Not Found: Endpoint not found. Endpoint: {endpoint}",
            System.Net.HttpStatusCode.BadRequest => 
                $"400 Bad Request: Invalid request parameters. Response: {errorContent}. Endpoint: {endpoint}",
            _ => 
                $"HTTP {(int)statusCode} {statusCode}: Request failed. Response: {errorContent}. Endpoint: {endpoint}"
        };
    }
}

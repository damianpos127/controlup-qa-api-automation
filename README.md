# ControlUp API Automation Framework

A production-grade C# automation framework for analyzing Binance market data via RapidAPI. This framework identifies the top 3 cryptocurrency symbols with the highest 24-hour price change percentage and generates comprehensive reports.

## Features

- **Clean Architecture**: Separation of concerns with dedicated layers for API clients, services, and reporting
- **Type-Safe API Client**: Strongly-typed DTOs and interfaces for all API interactions
- **Comprehensive Testing**: Integration tests with reliable assertions for volatile market data
- **Multi-Format Reporting**: Generates both JSON and Markdown reports
- **CI/CD Ready**: GitHub Actions pipeline for automated builds and tests
- **Production-Grade**: Proper error handling, logging, and configuration management

## Prerequisites

- .NET SDK 10.0 or later
- RapidAPI account with access to the Binance API endpoint
- Git (for cloning and CI/CD)

## Quick Start

### Prerequisites
- .NET SDK 10.0 or later
- RapidAPI account with access to the [Binance API endpoint](https://rapidapi.com/Glavier/api/binance43)
- Git (for cloning and CI/CD)

### Configuration

#### Step 1: Get Your RapidAPI Key
1. Go to [RapidAPI](https://rapidapi.com/)
2. Sign up or log in
3. Navigate to the [Binance API endpoint](https://rapidapi.com/Glavier/api/binance43)
4. Subscribe to the API (if required)
5. Copy your API key from the dashboard

#### Step 2: Configure Local Development

1. Copy the example configuration file:
   ```bash
   cp tests/ControlUp.ApiTests/appsettings.Local.json.example tests/ControlUp.ApiTests/appsettings.Local.json
   ```

2. Edit `tests/ControlUp.ApiTests/appsettings.Local.json` and add your RapidAPI key:
   ```json
   {
     "RapidApi": {
       "Key": "YOUR_RAPIDAPI_KEY_HERE"
     }
   }
   ```

   **Note**: `appsettings.Local.json` is git-ignored to prevent committing secrets.

### Environment Variables (Alternative)

You can also configure via environment variables:
- `RapidApi__Key`: Your RapidAPI key
- `RapidApi__Host`: RapidAPI host (default: `binance43.p.rapidapi.com`)
- `RapidApi__BaseUrl`: Base URL (default: `https://binance43.p.rapidapi.com`)

### CI/CD (GitHub Actions)

Configure the following secrets in your GitHub repository:
- `RAPIDAPI_KEY`: Your RapidAPI key
- `RAPIDAPI_HOST`: RapidAPI host (optional, defaults to `binance43.p.rapidapi.com`)
- `RAPIDAPI_BASE_URL`: Base URL (optional, defaults to `https://binance43.p.rapidapi.com`)

## Building

```bash
dotnet restore
dotnet build
```

## Running Tests

```bash
dotnet test
```

Or run with more verbose output:
```bash
dotnet test --verbosity normal
```

## Output

After running tests, reports are generated in the `artifacts/` directory:
- `report.json`: Machine-readable JSON format
- `report.md`: Human-readable Markdown table format

Example Markdown output:
```
# Top Market Movers Report

**Generated At:** 2026-02-27 09:48:00 UTC

## Top 3 Symbols by 24h Price Change

| Rank | Symbol | 24h Change % | Average Price |
|------|--------|--------------|---------------|
| 1    | BTCUSDT | 5.23%       | 43250.50000000 |
| 2    | ETHUSDT | 3.45%       | 2650.75000000 |
| 3    | BNBUSDT | 2.18%       | 315.25000000 |
```

## Project Structure

```
.
├── src/
│   └── ControlUp.Core/          # Core library
│       ├── Clients/              # API client implementations
│       ├── Configuration/        # Configuration models
│       ├── Models/                # DTOs and domain models
│       ├── Reporting/             # Report generation
│       └── Services/              # Business logic services
├── tests/
│   └── ControlUp.ApiTests/       # Integration tests
├── artifacts/                     # Generated reports
├── .github/
│   └── workflows/
│       └── ci.yml                # CI/CD pipeline
└── README.md
```

## Design Decisions

### Reliability Over Brittleness
- Tests assert **structure and sanity** rather than exact values (prices change constantly)
- Validates data types, ordering, and business rules
- Handles API failures gracefully with proper error handling

### Maintainability
- Clear separation of concerns (Client → Service → Reporting)
- Dependency injection for testability
- Comprehensive logging for debugging
- Type-safe models prevent runtime errors

### Extensibility
- Interface-based design allows easy mocking and testing
- Configuration-driven approach supports multiple environments
- Report generator can be extended to support additional formats

## AI Usage

This project was developed using **Cursor** as the primary AI coding assistant, demonstrating an AI-native development approach.

### AI Agents and Their Roles

#### 1. **Architecture Agent**
- **Task**: Designed the overall solution structure with proper separation of concerns
- **Contribution**: Suggested layered architecture (Clients → Services → Reporting)
- **Review**: Validated against SOLID principles and production best practices

#### 2. **API Contract Agent**
- **Task**: Analyzed RapidAPI Binance endpoint to infer DTO structures
- **Contribution**: Created strongly-typed models (`Ticker24HrDto`, `AvgPriceDto`) with proper JSON deserialization
- **Review**: Added null-safety checks, decimal parsing helpers, and error handling

#### 3. **Test Strategy Agent**
- **Task**: Designed reliable test assertions for volatile market data
- **Contribution**: Identified that exact price assertions would be brittle; focused on structure, ordering, and sanity checks
- **Review**: Implemented assertions for:
  - Data structure validity
  - Symbol uniqueness
  - Descending order by price change
  - Positive price values
  - Report generation verification

#### 4. **CI/CD Pipeline Agent**
- **Task**: Generated GitHub Actions workflow with proper secret management
- **Contribution**: Created workflow that builds, tests, and uploads artifacts
- **Review**: Ensured secrets are never committed, artifacts are preserved, and pipeline runs on multiple triggers

#### 5. **Code Quality Agent**
- **Task**: Ensured production-grade code quality throughout
- **Contribution**: Added comprehensive logging, error handling, and documentation
- **Review**: Applied consistent naming conventions, proper async/await patterns, and resource disposal

### What Was Changed After AI Generation

1. **Error Handling**: Enhanced API client error handling with specific exception types and logging
2. **Configuration**: Added support for both JSON files and environment variables
3. **Test Assertions**: Refined to focus on data structure rather than exact values
4. **Report Formatting**: Improved Markdown table formatting for better readability
5. **Logging**: Added structured logging throughout for better observability

### AI-Native Features Demonstrated

- **Cursor Rules**: Created `.cursor/rules/` with project-specific guidelines (see below)
- **Context-Aware Development**: AI understood the full context of the assessment requirements
- **Pattern Recognition**: AI identified common patterns (HttpClientFactory, DI, etc.) and applied them correctly
- **Iterative Refinement**: Multiple passes to refine code quality and test reliability

## Cursor Rules

This project includes Cursor-specific rules in `.cursor/rules/` that guide AI assistance:
- Architecture patterns and conventions
- Testing strategies for volatile data
- API client best practices
- Report generation standards

## Additional Documentation

- **[GITHUB_SETUP_GUIDE.md](GITHUB_SETUP_GUIDE.md)**: Step-by-step guide for GitHub repository setup and CI/CD configuration
- **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)**: Common issues and solutions
- **[.cursor/rules/](.cursor/rules/)**: AI development guidelines

## License

This project is created for assessment purposes.

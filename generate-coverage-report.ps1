# PowerShell script to generate coverage report with proper filtering
# Usage: powershell -ExecutionPolicy Bypass -File generate-coverage-report.ps1

# Clean up previous results
if (Test-Path -Path "WorkMood.MauiApp.Tests\TestResults" -PathType Container) {
    Remove-Item -Path "WorkMood.MauiApp.Tests\TestResults" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Removed WorkMood.MauiApp.Tests\TestResults folder" -ForegroundColor Green
}

if (Test-Path -Path "CoverageReport" -PathType Container) {
    Remove-Item -Path "CoverageReport" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Removed CoverageReport folder" -ForegroundColor Green
}

# Run tests with coverage collection
Write-Host "Running tests with coverage collection..." -ForegroundColor Cyan
dotnet test WorkMood.MauiApp.Tests --collect:"XPlat Code Coverage"

# Check if coverage data was generated
$coverageFiles = Get-ChildItem -Path "WorkMood.MauiApp.Tests\TestResults\**\coverage.cobertura.xml" -Recurse -ErrorAction SilentlyContinue

if ($coverageFiles.Count -eq 0) {
    Write-Host "ERROR: No coverage files found!" -ForegroundColor Red
    exit 1
}

Write-Host "Found $($coverageFiles.Count) coverage file(s)" -ForegroundColor Green

# Generate the coverage report with filters
Write-Host "Generating filtered coverage report..." -ForegroundColor Cyan
$reportsParam = "WorkMood.MauiApp.Tests\TestResults\**\coverage.cobertura.xml"
$targetParam = "CoverageReport"
$reportTypesParam = "TextSummary"
$verbosityParam = "Off"
$assemblyFiltersParam = "-whats-your-version"
$classFiltersParam = "-Microsoft.*;-__XamlGeneratedCode__.*;-WinRT.*;-*.Tests.*;-*.XamlTypeInfo.*;-*.WinUI.*"

& reportgenerator `
    "-reports:$reportsParam" `
    "-targetdir:$targetParam" `
    "-reporttypes:$reportTypesParam" `
    "-verbosity:$verbosityParam" `
    "-assemblyfilters:$assemblyFiltersParam" `
    "-classfilters:$classFiltersParam"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: reportgenerator failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Display the summary
if (Test-Path -Path "CoverageReport\Summary.txt") {
    Write-Host "`nCoverage Report:" -ForegroundColor Yellow
    Write-Host "=================" -ForegroundColor Yellow
    Get-Content "CoverageReport\Summary.txt"
} else {
    Write-Host "ERROR: Summary.txt not found in CoverageReport folder" -ForegroundColor Red
    exit 1
}
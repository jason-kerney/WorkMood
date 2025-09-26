#!/usr/bin/env pwsh
#
# PowerShell script to run the WorkMood MAUI app on Windows
# This script builds and runs the Windows version of the application
#

param(
    [switch]$Clean,
    [switch]$Release,
    [switch]$Help
)

if ($Help) {
    Write-Host "Usage: .\run-windows.ps1 [options]"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -Clean    Clean the solution before building"
    Write-Host "  -Release  Build and run in Release mode (default is Debug)"
    Write-Host "  -Help     Show this help message"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\run-windows.ps1              # Build and run in Debug mode"
    Write-Host "  .\run-windows.ps1 -Clean       # Clean, build and run in Debug mode"
    Write-Host "  .\run-windows.ps1 -Release     # Build and run in Release mode"
    Write-Host "  .\run-windows.ps1 -Clean -Release  # Clean, build and run in Release mode"
    exit 0
}

# Set script location as working directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

Write-Host "🏃 Running WorkMood MAUI App for Windows..." -ForegroundColor Cyan
Write-Host ""

# Determine configuration
$Configuration = if ($Release) { "Release" } else { "Debug" }
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Clean if requested
if ($Clean) {
    Write-Host "🧹 Cleaning solution..." -ForegroundColor Yellow
    try {
        dotnet clean "WorkMood.sln" --configuration $Configuration
        if ($LASTEXITCODE -ne 0) {
            throw "Clean failed"
        }
        Write-Host "✅ Clean completed successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Clean failed: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
    Write-Host ""
}

# Build the solution
Write-Host "🔨 Building solution..." -ForegroundColor Yellow
try {
    dotnet build "WorkMood.sln" --configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
    Write-Host "✅ Build completed successfully" -ForegroundColor Green
}
catch {
    Write-Host "❌ Build failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Run the MAUI app targeting Windows
Write-Host "🚀 Starting WorkMood MAUI App..." -ForegroundColor Yellow
try {
    # Run the MAUI app with Windows target framework
    dotnet run --project "MauiApp\WorkMood.MauiApp.csproj" --framework net9.0-windows10.0.19041.0 --configuration $Configuration
}
catch {
    Write-Host "❌ Failed to start application: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
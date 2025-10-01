# Cleanup script for test folders
# Quietly removes TestResults and CoverageReport folders if they exist
# 
# Usage: 
#   powershell -ExecutionPolicy Bypass -File cleanup-test-folders.ps1
#   or
#   Get-Content cleanup-test-folders.ps1 | powershell -Command -

# Check and remove WorkMood.MauiApp.Tests\TestResults folder
if (Test-Path -Path "WorkMood.MauiApp.Tests\TestResults" -PathType Container) {
    try {
        Remove-Item -Path "WorkMood.MauiApp.Tests\TestResults" -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "Removed WorkMood.MauiApp.Tests\TestResults folder" -ForegroundColor Green
    }
    catch {
        # Fail silently - no output on error
    }
}

# Check and remove CoverageReport folder
if (Test-Path -Path "CoverageReport" -PathType Container) {
    try {
        Remove-Item -Path "CoverageReport" -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "Removed CoverageReport folder" -ForegroundColor Green
    }
    catch {
        # Fail silently - no output on error
    }
}
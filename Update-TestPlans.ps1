# Test Plan Framework Update Script
# Updates all individual test plans to include xUnit testing framework specifications

$ErrorActionPreference = "Stop"

# Define the testing framework section to be inserted
$testingFrameworkSection = @"

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: ``using Xunit;``  
**Assertion Style**: ``Assert.NotNull()``, ``Assert.Equal()``, ``Assert.True()`` etc. (xUnit syntax)  
**Test Method Attributes**: ``[Fact]`` for single tests, ``[Theory]`` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (``[Test]``, ``Assert.That()``, ``Is.EqualTo()``, etc.)
"@

# Get all test plan files
$testPlanPath = "docs\testing-strategy\individual-plans"
$files = Get-ChildItem "$testPlanPath\*-TEST-PLAN.md"

Write-Host "Found $($files.Count) test plan files to update" -ForegroundColor Green

# Counters for tracking
$successCount = 0
$errorCount = 0
$skippedCount = 0
$errorFiles = @()

foreach ($file in $files) {
    try {
        Write-Host "Processing: $($file.Name)" -ForegroundColor Yellow
        
        # Read the file content
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        
        # Check if testing framework section already exists
        if ($content -match "## Testing Framework Requirements") {
            Write-Host "  -> Already has testing framework section, skipping" -ForegroundColor Cyan
            $skippedCount++
            continue
        }
        
        # Find the first heading (title line) - handle different patterns
        $lines = $content -split "`n"
        $insertIndex = -1
        
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $trimmedLine = $lines[$i].Trim()
            if ($trimmedLine -match "^# .+ - Individual Test Plan$" -or $trimmedLine -match "^# .+ Test Plan$") {
                $insertIndex = $i + 1
                break
            }
        }
        
        if ($insertIndex -eq -1) {
            Write-Host "  -> Could not find title heading, skipping" -ForegroundColor Red
            $errorCount++
            $errorFiles += $file.Name
            continue
        }
        
        # Insert the testing framework section
        $newLines = @()
        $newLines += $lines[0..($insertIndex-1)]
        $newLines += $testingFrameworkSection -split "`n"
        $newLines += $lines[$insertIndex..($lines.Count-1)]
        
        # Write the updated content back
        $newContent = $newLines -join "`n"
        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8 -NoNewline
        
        Write-Host "  -> Successfully updated" -ForegroundColor Green
        $successCount++
        
    } catch {
        Write-Host "  -> ERROR: $($_.Exception.Message)" -ForegroundColor Red
        $errorCount++
        $errorFiles += $file.Name
    }
}

# Summary report
Write-Host "`n=== UPDATE SUMMARY ===" -ForegroundColor Magenta
Write-Host "Total files processed: $($files.Count)" -ForegroundColor White
Write-Host "Successfully updated: $successCount" -ForegroundColor Green
Write-Host "Already had framework section: $skippedCount" -ForegroundColor Cyan
Write-Host "Errors encountered: $errorCount" -ForegroundColor Red

if ($errorFiles.Count -gt 0) {
    Write-Host "`nFiles with errors:" -ForegroundColor Red
    foreach ($errorFile in $errorFiles) {
        Write-Host "  - $errorFile" -ForegroundColor Red
    }
}

Write-Host "`nScript execution completed." -ForegroundColor Magenta
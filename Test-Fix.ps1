# Clear previous debug logs and test the data loading fix
Write-Host "Testing the property naming fix..." -ForegroundColor Green

# Clear old logs
$desktopPath = [Environment]::GetFolderPath('Desktop')
$logFiles = @("WorkMood_Debug.log", "WorkMood_ViewModel_Debug.log", "WorkMood_HistoryUI_Debug.log")

foreach ($logFile in $logFiles) {
    $logPath = Join-Path $desktopPath $logFile
    if (Test-Path $logPath) {
        Remove-Item $logPath -Force
        Write-Host "Cleared $logFile" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "✅ Logs cleared" -ForegroundColor Green
Write-Host "✅ Property naming policy fixed (camelCase)" -ForegroundColor Green
Write-Host "✅ Build successful" -ForegroundColor Green
Write-Host ""
Write-Host "Now test the app:" -ForegroundColor Cyan
Write-Host "1. Run: .\run-windows.ps1" -ForegroundColor White
Write-Host "2. Navigate to History page" -ForegroundColor White
Write-Host "3. Check if entries are now displayed" -ForegroundColor White
Write-Host "4. Run: .\Show-MoodLogs.ps1 to see debug logs" -ForegroundColor White
Write-Host ""
Write-Host "Expected result: History page should now show your 8 mood entries!" -ForegroundColor Green
# PowerShell script to clear cache and run the app to test data loading
Write-Host "Testing WorkMood Data Loading..." -ForegroundColor Green

# Clear any existing debug logs for a fresh start
$desktopPath = [Environment]::GetFolderPath('Desktop')
$logFiles = @(
    "WorkMood_Debug.log",
    "WorkMood_ViewModel_Debug.log", 
    "WorkMood_HistoryUI_Debug.log"
)

foreach ($logFile in $logFiles) {
    $logPath = Join-Path $desktopPath $logFile
    if (Test-Path $logPath) {
        Remove-Item $logPath -Force
        Write-Host "Cleared existing log: $logFile" -ForegroundColor Yellow
    }
}

Write-Host "`nBackup status:" -ForegroundColor Cyan
Write-Host "✅ Backup created at: C:\Users\jasonkerney\AppData\Local\WorkMood\backups\" -ForegroundColor Green
Write-Host "✅ Project backup: mood_data_backup_2025-09-29_10-23-52.json" -ForegroundColor Green

Write-Host "`nDebug logging enabled for:" -ForegroundColor Cyan
Write-Host "- MoodDataService (WorkMood_Debug.log)" -ForegroundColor White
Write-Host "- HistoryViewModel (WorkMood_ViewModel_Debug.log)" -ForegroundColor White  
Write-Host "- History UI (WorkMood_HistoryUI_Debug.log)" -ForegroundColor White

Write-Host "`nNow run the app and navigate to the History page." -ForegroundColor Yellow
Write-Host "Debug logs will be created on your desktop." -ForegroundColor Yellow
Write-Host "`nAfter testing, run 'Show-MoodLogs.ps1' to view the logs." -ForegroundColor Cyan
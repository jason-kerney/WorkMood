# PowerShell script to show mood debug logs
$desktopPath = [Environment]::GetFolderPath('Desktop')
$logFiles = @(
    "WorkMood_Debug.log",
    "WorkMood_ViewModel_Debug.log", 
    "WorkMood_HistoryUI_Debug.log"
)

Write-Host "=== WorkMood Debug Logs ===" -ForegroundColor Green

foreach ($logFile in $logFiles) {
    $logPath = Join-Path $desktopPath $logFile
    
    Write-Host "`n" + ("=" * 60) -ForegroundColor Cyan
    Write-Host "LOG FILE: $logFile" -ForegroundColor Yellow
    Write-Host ("=" * 60) -ForegroundColor Cyan
    
    if (Test-Path $logPath) {
        $content = Get-Content $logPath
        if ($content) {
            Write-Host $content -ForegroundColor White
        } else {
            Write-Host "Log file exists but is empty" -ForegroundColor Gray
        }
    } else {
        Write-Host "Log file does not exist" -ForegroundColor Red
    }
}

Write-Host "`n" + ("=" * 60) -ForegroundColor Green
Write-Host "End of Debug Logs" -ForegroundColor Green
Write-Host ("=" * 60) -ForegroundColor Green
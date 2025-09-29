# PowerShell script to backup the mood_data.json file
$sourceFile = "C:\Users\jasonkerney\AppData\Local\WorkMood\mood_data.json"
$backupDir = "C:\Users\jasonkerney\AppData\Local\WorkMood\backups"
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$backupFile = Join-Path $backupDir "mood_data_backup_$timestamp.json"

Write-Host "Creating backup of mood data..." -ForegroundColor Green
Write-Host "Source: $sourceFile" -ForegroundColor Cyan
Write-Host "Backup: $backupFile" -ForegroundColor Cyan

try {
    # Create backup directory if it doesn't exist
    if (-not (Test-Path $backupDir)) {
        New-Item -Path $backupDir -ItemType Directory -Force | Out-Null
        Write-Host "Created backup directory: $backupDir" -ForegroundColor Yellow
    }
    
    # Check if source file exists
    if (Test-Path $sourceFile) {
        # Copy the file to backup location
        Copy-Item -Path $sourceFile -Destination $backupFile -Force
        
        # Verify backup was created
        if (Test-Path $backupFile) {
            $originalSize = (Get-Item $sourceFile).Length
            $backupSize = (Get-Item $backupFile).Length
            
            if ($originalSize -eq $backupSize) {
                Write-Host "✅ Backup created successfully!" -ForegroundColor Green
                Write-Host "   Original size: $originalSize bytes" -ForegroundColor White
                Write-Host "   Backup size: $backupSize bytes" -ForegroundColor White
                Write-Host "   Backup location: $backupFile" -ForegroundColor White
                
                # Also create a backup in the project directory for easy access
                $projectBackup = "C:\Development\github\jason-kerney\WorkMood\mood_data_backup_$timestamp.json"
                Copy-Item -Path $sourceFile -Destination $projectBackup -Force
                Write-Host "   Project backup: $projectBackup" -ForegroundColor White
            } else {
                Write-Host "❌ Backup verification failed - sizes don't match!" -ForegroundColor Red
            }
        } else {
            Write-Host "❌ Backup file was not created!" -ForegroundColor Red
        }
    } else {
        Write-Host "❌ Source file does not exist: $sourceFile" -ForegroundColor Red
    }
    
} catch {
    Write-Host "❌ Error creating backup: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nBackup process completed." -ForegroundColor Green
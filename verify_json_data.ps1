# PowerShell script to verify the mood_data.json file structure
$jsonPath = "C:\Users\jasonkerney\AppData\Local\WorkMood\mood_data.json"

Write-Host "Checking mood_data.json file..." -ForegroundColor Green

if (Test-Path $jsonPath) {
    Write-Host "File exists at: $jsonPath" -ForegroundColor Green
    
    try {
        $jsonContent = Get-Content $jsonPath -Raw
        Write-Host "`nFile size: $($jsonContent.Length) characters" -ForegroundColor Cyan
        
        if ([string]::IsNullOrWhiteSpace($jsonContent)) {
            Write-Host "File is empty or contains only whitespace" -ForegroundColor Yellow
        } else {
            Write-Host "`nJSON Content Preview (first 1000 characters):" -ForegroundColor Cyan
            if ($jsonContent.Length -gt 1000) {
                Write-Host $jsonContent.Substring(0, 1000) -ForegroundColor White
                Write-Host "... (truncated)" -ForegroundColor Gray
            } else {
                Write-Host $jsonContent -ForegroundColor White
            }
            
            # Try to parse as JSON to check structure
            try {
                $parsedJson = $jsonContent | ConvertFrom-Json
                Write-Host "`nJSON is valid and can be parsed!" -ForegroundColor Green
                
                if ($parsedJson -is [Array]) {
                    Write-Host "Data is an array with $($parsedJson.Count) items" -ForegroundColor Green
                    
                    if ($parsedJson.Count -gt 0) {
                        Write-Host "`nFirst item structure:" -ForegroundColor Cyan
                        $firstItem = $parsedJson[0]
                        $firstItem | Get-Member -MemberType NoteProperty | ForEach-Object {
                            $propName = $_.Name
                            $propValue = $firstItem.$propName
                            Write-Host "  $propName : $propValue ($($propValue.GetType().Name))" -ForegroundColor White
                        }
                    }
                } else {
                    Write-Host "Data is not an array - this might be an issue" -ForegroundColor Yellow
                    Write-Host "Type: $($parsedJson.GetType())" -ForegroundColor Yellow
                }
                
            } catch {
                Write-Host "`nJSON parsing failed: $($_.Exception.Message)" -ForegroundColor Red
            }
        }
        
    } catch {
        Write-Host "Error reading file: $($_.Exception.Message)" -ForegroundColor Red
    }
    
} else {
    Write-Host "File does not exist at: $jsonPath" -ForegroundColor Red
}

Write-Host "`nExpected MoodEntry structure should have:" -ForegroundColor Yellow
Write-Host "- Date (string in yyyy-MM-dd format)" -ForegroundColor White
Write-Host "- StartOfWork (number or null)" -ForegroundColor White
Write-Host "- EndOfWork (number or null)" -ForegroundColor White
Write-Host "- CreatedAt (DateTime string)" -ForegroundColor White
Write-Host "- LastModified (DateTime string)" -ForegroundColor White
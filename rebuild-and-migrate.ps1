# Stop all dotnet processes and rebuild
Write-Host "Stopping all dotnet processes..." -ForegroundColor Yellow
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "TranstFeeds" -ErrorAction SilentlyContinue | Stop-Process -Force

Start-Sleep -Seconds 2

Write-Host "Cleaning project..." -ForegroundColor Cyan
dotnet clean

Start-Sleep -Seconds 1

Write-Host "Building project..." -ForegroundColor Cyan
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild successful!" -ForegroundColor Green
    
    Write-Host "`nChecking migrations..." -ForegroundColor Cyan
    dotnet ef migrations list
    
    Write-Host "`nApplying migrations..." -ForegroundColor Cyan
    dotnet ef database update
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nDatabase updated successfully!" -ForegroundColor Green
        Write-Host "`nYou can now run: dotnet run" -ForegroundColor Yellow
    } else {
        Write-Host "`nMigration failed. Check the error above." -ForegroundColor Red
    }
} else {
    Write-Host "`nBuild failed. Check the errors above." -ForegroundColor Red
}

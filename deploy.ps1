
$projectName = "SupportChat.Backend.csproj"  
$appPoolName = "ChatSupportApi"              
$iisPath     = "C:\inetpub\wwwroot\ChatSupportAPI"   
$publishPath = ".\bin\Release\net9.0\publish\" 

# Stop execution if any step fails
$ErrorActionPreference = "Stop"

Write-Host "🚀 Starting automated deployment to IIS..." -ForegroundColor Cyan

# 1. Stop the IIS App Pool so files aren't locked
Write-Host "🛑 Stopping IIS Application Pool: $appPoolName..." -ForegroundColor Yellow
Stop-WebAppPool -Name $appPoolName

# 2. Compile and Publish the .NET API
Write-Host "📦 Compiling and publishing project in Release mode..." -ForegroundColor Cyan
dotnet publish $projectName -c Release --nologo

# 3. Clean up old files in IIS (EXCEPT the logs folder)
Write-Host "🧹 Cleaning old files in $iisPath (preserving logs)..." -ForegroundColor Cyan
if (Test-Path $iisPath) {
    Get-ChildItem -Path $iisPath | Where-Object { $_.Name -ne "logs" } | Remove-Item -Recurse -Force
}

# 4. Copy the freshly published files to the IIS directory
Write-Host "🚚 Copying new files to IIS folder..." -ForegroundColor Green
Copy-Item "$publishPath\*" $iisPath -Recurse -Force

# 4a. Enable logging in the deployed web.config dynamically
$deployedWebConfig = "$iisPath\web.config"
if (Test-Path $deployedWebConfig) {
    Write-Host "📝 Turning on stdout logs in web.config..." -ForegroundColor Cyan
    $configContent = Get-Content $deployedWebConfig
    $configContent = $configContent -replace 'stdoutLogEnabled="false"', 'stdoutLogEnabled="true"'
    Set-Content $deployedWebConfig $configContent
}

# 4b. Ensure the logs directory exists
$logFolder = "$iisPath\logs"
if (-not (Test-Path $logFolder)) {
    New-Item -ItemType Directory -Force -Path $logFolder
}

# 4c. Grant the IIS App Pool write permissions to the logs directory
Write-Host "🔑 Applying folder permissions to logs folder..." -ForegroundColor Cyan
icacls $logFolder /grant "IIS AppPool\${appPoolName}:(OI)(CI)RXW" /q

# 5. Restart the App Pool to bring the API back online
Write-Host "▶️ Starting IIS Application Pool: $appPoolName..." -ForegroundColor Yellow
Start-WebAppPool -Name $appPoolName

Write-Host "🎉 Deployment finished successfully!" -ForegroundColor Green
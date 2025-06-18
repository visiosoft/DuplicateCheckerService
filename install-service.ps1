# Run this script as Administrator
$serviceName = "DuplicateCheckerService"
$displayName = "Directory Duplicate Checker Service"
$description = "Monitors a directory for file changes and detects duplicate files"
$exePath = Join-Path $PSScriptRoot "DuplicateCheckerService\bin\Release\net6.0\win-x64\publish\DuplicateCheckerService.exe"

# Stop and remove existing service if it exists
if (Get-Service $serviceName -ErrorAction SilentlyContinue) {
    Stop-Service $serviceName
    sc.exe delete $serviceName
}

# Create the service
sc.exe create $serviceName binPath= $exePath start= auto DisplayName= $displayName
sc.exe description $serviceName $description

# Set the service to start automatically
sc.exe config $serviceName start= auto

# Start the service
Start-Service $serviceName

Write-Host "Service installed successfully!"
Write-Host "Service Name: $serviceName"
Write-Host "Display Name: $displayName"
Write-Host "Description: $description"
Write-Host "Executable Path: $exePath" 
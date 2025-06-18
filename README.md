# Directory Duplicate Checker Service

A Windows Service that monitors a specified directory for file changes and detects duplicate files based on their contents.

## Features

- Monitors a specified directory for file changes (creation, modification, deletion)
- Logs all file events to a text file
- Detects files with duplicate contents using SHA-256 hashing
- Runs automatically after system reboot
- Configurable monitoring interval and buffer size

## Prerequisites

- Windows operating system
- .NET 6.0 SDK or later
- Administrator privileges for installation

## Building the Service

1. Clone the repository:
   ```
   git clone [repository-url]
   cd DuplicateCheckerService
   ```

2. Build the service as a single executable:
   ```
   cd DuplicateCheckerService
   dotnet publish -c Release
   ```

   The executable will be created at:
   `DuplicateCheckerService\bin\Release\net6.0\win-x64\publish\DuplicateCheckerService.exe`

## Installation

### Method 1: Using PowerShell Script (Recommended)

1. Open PowerShell as Administrator
2. Navigate to the service directory:
   ```
   cd path\to\DuplicateCheckerService
   ```
3. Run the installation script:
   ```
   .\install-service.ps1
   ```

### Method 2: Manual Installation

1. Copy the following files to your desired location:
   - `DuplicateCheckerService.exe` (from the publish directory)
   - `appsettings.json` (from the publish directory)

2. Open PowerShell as Administrator and run:
   ```
   sc.exe create DuplicateCheckerService binPath= "path\to\DuplicateCheckerService.exe" start= auto DisplayName= "Directory Duplicate Checker Service"
   sc.exe description DuplicateCheckerService "Monitors a directory for file changes and detects duplicate files"
   sc.exe start DuplicateCheckerService
   ```

## Configuration

The service can be configured by modifying the `appsettings.json` file:

```json
{
  "ServiceSettings": {
    "WatchDirectory": "D:\\WatchDirectory",  // Directory to monitor
    "LogFilePath": "D:\\ServiceLogs\\log.txt",  // Path to log file
    "CheckIntervalSeconds": 60,  // Interval between duplicate checks
    "HashBufferSize": 81920  // Buffer size for file hashing (80KB)
  }
}
```

## Verifying Installation

1. Open Windows Services (services.msc)
2. Look for "Directory Duplicate Checker Service"
3. Verify that the service is running
4. Check the configured log file for activity

## Logging

The service logs the following events to the specified log file:
- File creation
- File modification
- File deletion
- Duplicate file detection

Log entries include timestamps and full file paths.

## Performance Considerations

- The service uses SHA-256 hashing to detect duplicate files
- File hashing is performed asynchronously to prevent blocking
- A configurable buffer size is used for file reading
- Duplicate checks are performed at configurable intervals

## Troubleshooting

1. Check the Windows Event Viewer for service-related errors
2. Verify the log file exists and is writable
3. Ensure the monitored directory exists and is accessible
4. Check service permissions in Windows Services

## Uninstallation

To uninstall the service, run the following command as Administrator:
```
sc.exe delete DuplicateCheckerService
```

## Development

To modify and rebuild the service:

1. Make your changes to the source code
2. Build the service:
   ```
   dotnet publish -c Release
   ```
3. Copy the new executable and configuration files to your service location
4. Restart the service:
   ```
   sc.exe stop DuplicateCheckerService
   sc.exe start DuplicateCheckerService
   ```

## Example

Here's an example of how the service detects duplicate files. I copied two files with the same image content but different names:

![image](https://github.com/user-attachments/assets/1a4432d5-9acf-41fa-8562-a0b906857e04)

The service detected these as duplicates:

![image](https://github.com/user-attachments/assets/02045ff7-1c88-49ca-8053-9605e9790277)

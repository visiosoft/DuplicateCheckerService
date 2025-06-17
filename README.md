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
- .NET 6.0 or later
- Administrator privileges for installation

## Installation

1. Build the solution:
   ```
   dotnet publish -c Release -r win-x64 --self-contained true
   ```

2. Run the installation script as Administrator:
   ```
   .\install-service.ps1
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
![image](https://github.com/user-attachments/assets/1a4432d5-9acf-41fa-8562-a0b906857e04)


Here i copied two files with same images with differnet names but same content  
![image](https://github.com/user-attachments/assets/02045ff7-1c88-49ca-8053-9605e9790277)


using System;

namespace DuplicateCheckerService.Configuration
{
    public class ServiceSettings
    {
        public string WatchDirectory { get; set; } = @"C:\WatchDirectory";
        public string LogFilePath { get; set; } = @"C:\ServiceLogs\log.txt";
        public int CheckIntervalSeconds { get; set; } = 60;
        public int HashBufferSize { get; set; } = 81920; // 80KB buffer for file hashing
    }
} 
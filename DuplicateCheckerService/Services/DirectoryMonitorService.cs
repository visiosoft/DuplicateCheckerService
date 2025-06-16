using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DuplicateCheckerService.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DuplicateCheckerService.Services
{
    public class DirectoryMonitorService : BackgroundService
    {
        private readonly ILogger<DirectoryMonitorService> _logger;
        private readonly ServiceSettings _settings;
        private readonly FileSystemWatcher _watcher;
        private readonly Dictionary<string, string> _fileHashes;

        public DirectoryMonitorService(
            ILogger<DirectoryMonitorService> logger,
            ServiceSettings settings)
        {
            _logger = logger;
            _settings = settings;
            _fileHashes = new Dictionary<string, string>();

            // Ensure log directory exists
            var logDirectory = Path.GetDirectoryName(_settings.LogFilePath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Ensure watch directory exists
            if (!string.IsNullOrEmpty(_settings.WatchDirectory) && !Directory.Exists(_settings.WatchDirectory))
            {
                Directory.CreateDirectory(_settings.WatchDirectory);
            }

            _watcher = new FileSystemWatcher(_settings.WatchDirectory)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnFileCreated;
            _watcher.Deleted += OnFileDeleted;
            _watcher.Changed += OnFileChanged;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Directory Monitor Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckForDuplicates();
                    await Task.Delay(TimeSpan.FromSeconds(_settings.CheckIntervalSeconds), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking for duplicates");
                    await LogToFile($"Error: {ex.Message}");
                }
            }
        }

        private async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                await LogToFile($"File created: {e.FullPath}");
                await UpdateFileHash(e.FullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing file creation: {e.FullPath}");
            }
        }

        private async void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                await LogToFile($"File deleted: {e.FullPath}");
                _fileHashes.Remove(e.FullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing file deletion: {e.FullPath}");
            }
        }

        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                await LogToFile($"File modified: {e.FullPath}");
                await UpdateFileHash(e.FullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing file modification: {e.FullPath}");
            }
        }

        private async Task UpdateFileHash(string filePath)
        {
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hash = await Task.Run(() => sha256.ComputeHash(stream));
                var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                _fileHashes[filePath] = hashString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error computing hash for file: {filePath}");
                throw;
            }
        }

        private async Task CheckForDuplicates()
        {
            var duplicateGroups = _fileHashes
                .GroupBy(x => x.Value)
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var group in duplicateGroups)
            {
                var files = string.Join(", ", group.Select(x => x.Key));
                await LogToFile($"Duplicate files detected with hash {group.Key}: {files}");
            }
        }

        private async Task LogToFile(string message)
        {
            try
            {
                var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                await File.AppendAllTextAsync(_settings.LogFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing to log file");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            await base.StopAsync(cancellationToken);
        }
    }
} 
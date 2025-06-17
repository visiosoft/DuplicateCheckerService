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
using Serilog;

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
                Log.Information("Created watch directory: {WatchDirectory}", _settings.WatchDirectory);
            }

            _watcher = new FileSystemWatcher(_settings.WatchDirectory)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnFileCreated;
            _watcher.Deleted += OnFileDeleted;
            _watcher.Changed += OnFileChanged;

            Log.Information("Directory Monitor Service initialized. Watching directory: {WatchDirectory}", _settings.WatchDirectory);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("Directory Monitor Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckForDuplicates();
                    await Task.Delay(TimeSpan.FromSeconds(_settings.CheckIntervalSeconds), stoppingToken);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error occurred while checking for duplicates");
                }
            }
        }

        private async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                Log.Information("File created: {FilePath}", e.FullPath);
                await UpdateFileHash(e.FullPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing file creation: {FilePath}", e.FullPath);
            }
        }

        private async void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                Log.Information("File deleted: {FilePath}", e.FullPath);
                _fileHashes.Remove(e.FullPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing file deletion: {FilePath}", e.FullPath);
            }
        }

        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                Log.Information("File modified: {FilePath}", e.FullPath);
                await UpdateFileHash(e.FullPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing file modification: {FilePath}", e.FullPath);
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
                Log.Debug("Updated hash for file: {FilePath} - Hash: {Hash}", filePath, hashString);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error computing hash for file: {FilePath}", filePath);
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
                Log.Warning("Duplicate files detected with hash {Hash}: {Files}", group.Key, files);
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
            Log.Information("Directory Monitor Service is stopping.");
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            await base.StopAsync(cancellationToken);
        }
    }
} 
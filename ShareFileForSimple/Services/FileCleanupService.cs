using Microsoft.EntityFrameworkCore;
using ShareFileForSimple.DataPersistent;

namespace ShareFileForSimple.Services
{
    public class FileCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _config;
        private readonly ILogger<FileCleanupService> _logger;
        private FileSystemWatcher? _watcher;
        private readonly string _uploadRoot;
        private readonly long _maxSizeBytes;

        public FileCleanupService(IServiceProvider serviceProvider, IConfiguration config, ILogger<FileCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _config = config;
            _logger = logger;

            _uploadRoot = Path.GetFullPath(_config.GetValue<string>("StorageSettings:UploadPath", "wwwroot/uploads"));
            _maxSizeBytes = _config.GetValue<long>("StorageSettings:MaxStorageSizeMB", 5000) * 1024 * 1024;

            // 确保目录存在
            if (!Directory.Exists(_uploadRoot)) Directory.CreateDirectory(_uploadRoot);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _watcher = new FileSystemWatcher(_uploadRoot)
            {
                IncludeSubdirectories = true, // 监控 yyyy-MM-dd 子目录
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            // 绑定事件：当文件创建或写入完成时触发
            _watcher.Created += (s, e) => OnFileChanged(stoppingToken);
            _watcher.Changed += (s, e) => OnFileChanged(stoppingToken);

            _logger.LogInformation($"OS级监控已启动：{_uploadRoot}，阈值：{_maxSizeBytes / 1024 / 1024}MB");

            // 保持服务运行
            return Task.CompletedTask;
        }

        private void OnFileChanged(CancellationToken ct)
        {
            // 这里的逻辑最好加个简易的“防抖”，避免大文件写入过程中频繁触发
            // 但为了逻辑清晰，我们直接进行大小检查
            CheckAndCleanup(ct).GetAwaiter().GetResult();
        }

        private async Task CheckAndCleanup(CancellationToken ct)
        {
            try
            {
                while (GetDirectorySize(_uploadRoot) > _maxSizeBytes)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var oldestRecord = await db.FileRecordModels
                        .Include(r => r.FileItems)
                        .OrderBy(r => r.CreateDateTime)
                        .FirstOrDefaultAsync(ct);

                    if (oldestRecord == null) break;

                    _logger.LogWarning($"OS监控发现空间溢出，自动清理: {oldestRecord.FileDescription}");

                    // 执行物理删除
                    string dateFolder = oldestRecord.CreateDateTime.ToString("yyyy-MM-dd");
                    foreach (var item in oldestRecord.FileItems)
                    {
                        string filePath = Path.Combine(_uploadRoot, dateFolder, item.StorageFileName);
                        if (File.Exists(filePath)) File.Delete(filePath);
                    }

                    db.FileRecordModels.Remove(oldestRecord);
                    await db.SaveChangesAsync(ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"实时清理失败: {ex.Message}");
            }
        }

        private long GetDirectorySize(string path)
        {
            return new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
        }

        public override void Dispose()
        {
            _watcher?.Dispose();
            base.Dispose();
        }
    }
}
using ShareFileForSimple.DataPersistent;

namespace ShareFileForSimple.Services
{
    public class FileCleanupService : BackgroundService
    {
        private readonly string _root;
        private readonly IServiceProvider _sp;
        public FileCleanupService(IConfiguration cfg, IServiceProvider sp)
        {
            _root = cfg["UploadSettings:StorageRoot"] ?? "wwwroot/uploads";
            _sp = sp;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var today = DateTime.Now.ToString("yyyy-MM-dd");
                Directory.CreateDirectory(Path.Combine(_root, today));
            }
        }
    }
}

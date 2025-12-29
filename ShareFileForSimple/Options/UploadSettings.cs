namespace ShareFileForSimple.Options
{
    public class UploadSettings
    {
        public int MaxStorageSizeMB { get; set; }
        public int SingleUploadMaxStorageSizeMB { get; set; }
        public string StorageRoot { get; set; } = string.Empty;
    }
}

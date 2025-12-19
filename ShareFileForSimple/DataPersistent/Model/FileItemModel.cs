using System.ComponentModel.DataAnnotations;

namespace ShareFileForSimple.DataPersistent.Model
{
    public class FileItemModel
    {
        [Key]
        public Guid Id { get; set; }
        public string FileName { get; set; } = "";// 原始文件名（如：作业.pdf）
        public string StorageFileName { get; set; } = ""; // 磁盘文件名（如：guid.pdf）
        public FileRecordModel? FileRecord { get; set; }
        public Guid FileRecordId { get; set; } // 外键
    }
}

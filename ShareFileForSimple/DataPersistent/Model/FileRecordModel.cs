using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;

namespace ShareFileForSimple.DataPersistent.Model
{
    public class FileRecordModel
    {
        [Key]
        public Guid Key { get; set; }
        public string FileDescription { get; set; } = "";
        public DateTime CreateDateTime { get; set; }
        public ICollection<FileItemModel> FileItems { get; set; } = new List<FileItemModel>();
    }
}

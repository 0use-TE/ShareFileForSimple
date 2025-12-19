using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareFileForSimple.DataPersistent;
using ShareFileForSimple.DataPersistent.Model;
using System.IO.Compression;
namespace ShareFileForSimple.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class UploadController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly string _storageRoot;

    public UploadController(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _storageRoot = config["UploadSettings:StorageRoot"] ?? "wwwroot/uploads";
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> PostFiles([FromForm] string description, [FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0) return BadRequest("No files.");

        var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
        var savePath = Path.Combine(_storageRoot, dateFolder);

        var record = new FileRecordModel { FileDescription = description, CreateDateTime=DateTime.Now };

        foreach (var file in files)
        {
            var fileGuidName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(savePath, fileGuidName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            record.FileItems.Add(new FileItemModel
            {
                FileName = file.FileName,
                StorageFileName = fileGuidName
            });
        }

        _context.FileRecordModels.Add(record);
        await _context.SaveChangesAsync();
        return Ok();
    }
    [HttpGet("{recordKey}")]
    public async Task<IActionResult> DownloadAll(Guid recordKey)
    {
        var record = await _context.FileRecordModels
            .Include(r => r.FileItems)
            .FirstOrDefaultAsync(r => r.Key == recordKey);

        if (record == null || !record.FileItems.Any()) return NotFound();

        using (var ms = new MemoryStream())
        {
            // 创建 Zip 归档
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var item in record.FileItems)
                {
                    var dateFolder = record.CreateDateTime.ToString("yyyy-MM-dd");
                    var filePath = Path.Combine(_storageRoot, dateFolder, item.StorageFileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        // 在 Zip 中创建文件，并使用原始文件名
                        var entry = archive.CreateEntry(item.FileName);
                        using (var entryStream = entry.Open())
                        using (var fileStream = System.IO.File.OpenRead(filePath))
                        {
                            await fileStream.CopyToAsync(entryStream);
                        }
                    }
                }
            }
            ms.Position = 0;
            return File(ms.ToArray(), "application/zip", $"{record.FileDescription}.zip");
        }
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> Download(Guid id)
    {
        var item = await _context.FileItemModels.Include(f => f.FileRecord)
                                 .FirstOrDefaultAsync(f => f.Id == id);
        if (item == null) return NotFound();

        var filePath = Path.Combine(_storageRoot, item.FileRecord!.CreateDateTime.ToString("yyyy-MM-dd"), item.StorageFileName);
        if (!System.IO.File.Exists(filePath)) return NotFound("文件已过期删除");

        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(bytes, "application/octet-stream", item.FileName);
    }
}

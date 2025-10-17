using Microsoft.AspNetCore.Http;

namespace SGE.Core.Entities;

public class FileUploadModel
{
    public IFormFile File { get; set; }
}
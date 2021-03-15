using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileWatchApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UploadController:ControllerBase
    {
        private readonly IConfiguration _configuration;
        public UploadController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost]
        [DisableRequestSizeLimit]//取消大小限制
        public async Task PostFile([FromForm]IFormFile file)
        {
            if (file == null)
            {
                return;
            }
            var path = _configuration["FileSavePath"];
            var filePath = Path.Combine(path,file.FileName);
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
        }
    }
}

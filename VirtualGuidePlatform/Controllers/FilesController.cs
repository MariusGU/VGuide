using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VirtualGuidePlatform.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private const string PathToServiceAccountKeyFile = "circular-jet-345211-05dcd8d32504.json";
        private const string ServiceAccountEmail = "vvista@circular-jet-345211.iam.gserviceaccount.com";

        private readonly IWebHostEnvironment _env;

        public FilesController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        [Route("uploadpicture")]
        public async Task<IActionResult> UploadePicture(IFormFile picture)
        {
            if(picture.Length > 0)
            {
                string directoryPath = Path.Combine(_env.ContentRootPath, "Images");
                string filePath = Path.Combine(directoryPath, picture.FileName);
                using(var stream = new FileStream(filePath, FileMode.Create))
                {
                    await picture.CopyToAsync(stream);
                }
                return Ok("Ikelta");
            }
            else
            {
                return BadRequest("Nepasirinktas failas");
            }
        }
        [HttpPost]
        [Route("uploadvideo")]
        public async Task<IActionResult> UploadeVideo(IFormFile video)
        {
            if (video.Length > 0)
            {
                string directoryPath = Path.Combine(_env.ContentRootPath, "Videos");
                string filePath = Path.Combine(directoryPath, video.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await video.CopyToAsync(stream);
                }
                return Ok("Ikelta");
            }
            else
            {
                return BadRequest("Nepasirinktas failas");
            }
        }
    }
}
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
using VirtualGuidePlatform.Data.Entities;
using VirtualGuidePlatform.Data.Entities.Blocks;

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
        [Route("test")]
        public async Task<IActionResult> TestPicture([FromForm] PostGuide guide)
        {
            int imgId = 0;
            int videoId = 0; 
            int textId = 0;
            List<BlockDB> dbBlocks = new List<BlockDB>();
            guide.DeserializeBlocks();

            BlockDB dbBlock;

            for (int i = 0; i < guide.DeserializedBlocks.Count; i++)
            {
                var block = guide.DeserializedBlocks[i];
                
                switch (block.Type)
                {
                    case "Text":
                        TextBlock textBlock = new TextBlock
                        {
                            ID = i,
                            Text = guide.Texts[textId++]
                        };
                        dbBlock = new BlockDB
                        {
                            Type = "Text",
                            txtBlock = textBlock
                        };
                        dbBlocks.Add(dbBlock);
                        break;
                    case "Video":
                        VideoBlock videoBlock = new VideoBlock
                        {
                            ID = i,
                            URI = "/Videos/" + guide.Videos[videoId].FileName,
                            FileName = guide.Videos[videoId].FileName
                        };
                        dbBlock = new BlockDB
                        {
                            Type = "Video",
                            vidBlock = videoBlock
                        };
                        videoId++;
                        dbBlocks.Add(dbBlock);
                        break;
                    case "Image":
                        ImageBlock imageBlock = new ImageBlock
                        {
                            ID = i,
                            URI = "/Images/" + guide.Images[imgId].FileName,
                            FileName = guide.Images[imgId].FileName
                        };
                        imgId++;
                        dbBlock = new BlockDB
                        {
                            Type = "Image",
                            imgBlock = imageBlock
                        };
                        dbBlocks.Add(dbBlock);
                        break;
                }
            }

            GuideDB guideDB = new GuideDB { blocks = dbBlocks };

            foreach(var item in guideDB.blocks)
            {
                Console.WriteLine(item.Type);
            }

            return Ok("");
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
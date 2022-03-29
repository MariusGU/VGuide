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
using VirtualGuidePlatform.Data.Repositories;

namespace VirtualGuidePlatform.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private const string PathToServiceAccountKeyFile = "circular-jet-345211-05dcd8d32504.json";
        private const string ServiceAccountEmail = "vvista@circular-jet-345211.iam.gserviceaccount.com";

        private readonly IWebHostEnvironment _env;

        private IGuidesRepository _guidesRepository;
        private readonly IBlocksRepository _blocksRepository;

        public FilesController(IWebHostEnvironment env, IGuidesRepository guidesRepository, IBlocksRepository blocksRepository)
        {
            _env = env;
            _guidesRepository = guidesRepository;
            _blocksRepository = blocksRepository;
        }
        private async Task<List<BlockDB>> FormaGotGuidePost([FromForm] PostGuide guide)
        {
            int imgId = 0;
            int videoId = 0;
            int textId = 0;
            List<BlockDB> dbBlocks = new List<BlockDB>();
            guide.DeserializeBlocks();

            Guides guide1 = new Guides()
            {
                gCreatorId = 1,
                locationXY = "dsadsad",
                city = "Kaunas",
                name = "Generuotas",
                language = "LT",
                uDate = DateTime.Now,
                price = 1.99
            };

            var created = await _guidesRepository.CreateGuide(guide1);

            string guideId = created._id;

            Console.WriteLine(guideId);

            BlockDB dbBlock;

            for (int i = 0; i < guide.DeserializedBlocks.Count; i++)
            {
                var block = guide.DeserializedBlocks[i];

                switch (block.Type)
                {
                    case "Text":
                        Tblocks textBlock = new Tblocks
                        {
                            text = guide.Texts[textId++],
                            priority = block.ID,
                            gId = created._id
                        };

                        await _blocksRepository.CreateTblock(textBlock);

                        dbBlock = new BlockDB
                        {
                            Type = "Text",
                            txtBlock = textBlock
                        };
                        dbBlocks.Add(dbBlock);
                        break;
                    case "Video":
                        string dirPathV = Path.Combine(_env.ContentRootPath, "Videos");
                        string[] splitTypeV = guide.Videos[videoId].ContentType.Split('/');
                        string combinedV = created._id + "v" + videoId.ToString() + "." + splitTypeV[1];
                        string filePathV = Path.Combine(dirPathV, combinedV);
                        Vblocks videoBlock = new Vblocks
                        {
                            priority = i,
                            URI = filePathV,
                            FileName = combinedV,
                            gId = created._id
                        };
                        using (var stream = new FileStream(filePathV, FileMode.Create))
                        {
                            await guide.Videos[videoId].CopyToAsync(stream);
                        }

                        await _blocksRepository.CreateVblock(videoBlock);

                        dbBlock = new BlockDB
                        {
                            Type = "Video",
                            vidBlock = videoBlock
                        };
                        videoId++;
                        dbBlocks.Add(dbBlock);
                        break;
                    case "Image":
                        string dirPathP = Path.Combine(_env.ContentRootPath, "Images");
                        Console.WriteLine(guide.Images[imgId].ContentType);
                        string[] splitTypeP = guide.Images[imgId].ContentType.Split('/');
                        Console.WriteLine(splitTypeP[1]);
                        string combinedP = created._id + "t" + imgId.ToString() + "." + splitTypeP[1];
                        string filePathP = Path.Combine(dirPathP, combinedP);
                        Pblocks imageBlock = new Pblocks
                        {
                            priority = i,
                            URI = filePathP,
                            FileName = combinedP,
                            gId = created._id
                        };

                        using (var stream = new FileStream(filePathP, FileMode.Create))
                        {
                            await guide.Images[imgId].CopyToAsync(stream);
                        }

                        await _blocksRepository.CreatePblock(imageBlock);

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
            return dbBlocks;
        }

        [HttpPost]
        [Route("test")]
        public async Task<IActionResult> TestPicture([FromForm] PostGuide guide)
        {
            List<BlockDB> dbBlocks = await FormaGotGuidePost(guide);

            Console.WriteLine(dbBlocks.Count);

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
                //string filePath = Path.Combine(directoryPath, picture.FileName);

                //failo vardo keitimas
                string[] splitType = picture.FileName.Split('.');
                string combined = "gidoid." + splitType[1];

                Console.WriteLine("Failo vardas yra " + combined);

                string filePath = Path.Combine(directoryPath, combined);
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
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
        private readonly IWebHostEnvironment _env;
        private IGuidesRepository _guidesRepository;
        private readonly IBlocksRepository _blocksRepository;

        public FilesController(IWebHostEnvironment env, IGuidesRepository guidesRepository, IBlocksRepository blocksRepository)
        {
            _env = env;
            _guidesRepository = guidesRepository;
            _blocksRepository = blocksRepository;
        }
        private async Task<ActionResult<Guides>> FormaGotGuidePost([FromForm] PostGuide guide)
        {
            int imgId = 0;
            int videoId = 0;
            int textId = 0;
            guide.DeserializeBlocks();

            Console.WriteLine("Title: {0}", guide.Title);
            Console.WriteLine("Description: {0}", guide.Description);
            Console.WriteLine("Price: {0}", guide.Price);
            Console.WriteLine("Language: {0}", guide.Language);

            Guides guide1 = new Guides()
            {
                gCreatorId = 1,
                locationXY = "dsadsad",
                description = guide.Description,
                city = "Kaunas",
                name = guide.Title,
                language = guide.Language,
                uDate = DateTime.Now,
                price = 1.99
            };
            var created = await _guidesRepository.CreateGuide(guide1);

            for (int i = 0; i < guide.DeserializedBlocks.Count; i++)
            {
                var block = guide.DeserializedBlocks[i];

                switch (block.Type)
                {
                    //---------------------------------------------------------------------------
                    //teksto bloko sukurimas ir issugojimas i duomenu baze
                    case "Text":
                        Tblocks textBlock = new Tblocks
                        {
                            text = guide.Texts[textId++],
                            priority = block.ID,
                            gId = created._id
                        };
                        await _blocksRepository.CreateTblock(textBlock);
                        break;
                        //-----------------------------------------------------------------------
                        //Video bloko sukurimas i duomenu baze ir failo ikelimas i Video folderi
                        //pakeitus jo pavadinima i gido id + v + video failo eiles numeris +
                        // + failo tipo prierasas (mp4, ....)
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
                            contentType = guide.Videos[videoId].ContentType,
                            gId = created._id
                        };
                        using (var stream = new FileStream(filePathV, FileMode.Create))
                        {
                            await guide.Videos[videoId].CopyToAsync(stream);
                        }
                        await _blocksRepository.CreateVblock(videoBlock);
                        videoId++;
                        break;
                        //------------------------------------------------------------------------
                        //Nuotraukos bloko sukurimas i duomenu baze, nuotraukos issaugojimas i 
                        // Image aplanka pakeitus pavadinima i gido id + p + nuotraukos eiles numeris
                        // nuotrauku failu eileje + nuotraukos failo tipas (png, jpg, ...)
                    case "Image":
                        string dirPathP = Path.Combine(_env.ContentRootPath, "Images");
                        Console.WriteLine(guide.Images[imgId].ContentType);
                        string[] splitTypeP = guide.Images[imgId].ContentType.Split('/');
                        Console.WriteLine(splitTypeP[1]);
                        string combinedP = created._id + "p" + imgId.ToString() + "." + splitTypeP[1];
                        string filePathP = Path.Combine(dirPathP, combinedP);
                        Pblocks imageBlock = new Pblocks
                        {
                            priority = i,
                            URI = filePathP,
                            FileName = combinedP,
                            contentType = guide.Images[imgId].ContentType,
                            gId = created._id
                        };
                        using (var stream = new FileStream(filePathP, FileMode.Create))
                        {
                            await guide.Images[imgId].CopyToAsync(stream);
                        }
                        await _blocksRepository.CreatePblock(imageBlock);
                        imgId++;
                        break;
                        //-------------------------------------------------------------------------
                }
            }
            return Created("sukurta", guide1);
        }

        private async Task<string> UploadPicture(IFormFile image)
        {
            string GDriveKeyPath = Path.Combine(_env.ContentRootPath, "GDriveCredentials.json");
            string DirectoryId = "1I-YrtKmGKyb9Ex-6U_nYwJw-2ynYTo5E";

            string filePath = Path.Combine(Path.Combine(_env.ContentRootPath, "Images"), "624575a2075fa8cc9616271cp0.jpeg");

            var credentials = GoogleCredential.FromFile(GDriveKeyPath).CreateScoped(DriveService.ScopeConstants.Drive);

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials
            });

            var fileMetaData = new Google.Apis.Drive.v3.Data.File()
            {
                Name = "Naujas.jpeg",
                Parents = new List<string>() { DirectoryId }
            };

            string uploadFileId;

            await using (var fsSource = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var request = service.Files.Create(fileMetaData, fsSource, "image/jpeg");
                request.Fields = "*";
                var results = await request.UploadAsync();

                if (results.Status == Google.Apis.Upload.UploadStatus.Failed)
                {
                    return "";
                }
                uploadFileId = request.ResponseBody?.Id;
                Console.WriteLine(uploadFileId);
            }
            return uploadFileId;
        }

        [HttpPost]
        [Route("test")]
        public async Task<IActionResult> TestPicture([FromForm] PostGuide guide)
        {
            await FormaGotGuidePost(guide);

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
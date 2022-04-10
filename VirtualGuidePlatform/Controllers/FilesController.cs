using Firebase.Auth;
using Firebase.Storage;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
        private IConfiguration _configuration;

        public FilesController(IWebHostEnvironment env, IGuidesRepository guidesRepository, IBlocksRepository blocksRepository, IConfiguration configuration)
        {
            _env = env;
            _guidesRepository = guidesRepository;
            _blocksRepository = blocksRepository;
            _configuration = configuration;
        }
        private async Task<ActionResult<Guides>> FormaGotGuidePost([FromForm] PostGuide guide)
        {
            Console.WriteLine("ieina");
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

                        var videoFileID = await UploadFileToFirebase(guide.Videos[videoId], combinedV, "videos");
                        Console.WriteLine(videoFileID);
                        if (videoFileID == "")
                        {
                            return BadRequest("Nepavyko ikelti video i Google drive");
                        }

                        Vblocks videoBlock = new Vblocks
                        {
                            priority = i,
                            URI = videoFileID,
                            FileName = combinedV,
                            contentType = guide.Videos[videoId].ContentType,
                            gId = created._id
                        };
                        //using (var stream = new FileStream(filePathV, FileMode.Create))
                        //{
                        //    await guide.Videos[videoId].CopyToAsync(stream);
                        //}
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

                        //Ikelia faila i google drive
                        var pictureFileID = await UploadFileToFirebase(guide.Images[imgId], combinedP, "pictures");
                        if(pictureFileID == "")
                        {
                            return BadRequest("Nepavyko ikelti paveikslelio i Google drive");
                        }

                        //sukuria picture block ikelimui i duombaze
                        Pblocks imageBlock = new Pblocks
                        {
                            priority = i,
                            URI = pictureFileID,
                            FileName = combinedP,
                            contentType = guide.Images[imgId].ContentType,
                            gId = created._id
                        };

                        await _blocksRepository.CreatePblock(imageBlock);
                        imgId++;
                        break;
                        //-------------------------------------------------------------------------
                }
            }
            return Created("sukurta", guide1);
        }

        public async Task<string> UploadFileToFirebase(IFormFile file, string newname, string type)
        {
            Stream stream; /*= picture.OpenReadStream();*/
            if (file.Length > 0)
            {
                //stream = new FileStream(Path.GetFullPath(picture.FileName), FileMode.Open);
                stream = file.OpenReadStream();
                var auth = new FirebaseAuthProvider(new FirebaseConfig(_configuration.GetConnectionString("FirebaseApiKey")));
                var a = await auth.SignInWithEmailAndPasswordAsync(_configuration.GetConnectionString("FirebaseEmail"), _configuration.GetConnectionString("FirebasePass"));

                var cancellation = new CancellationTokenSource();


                var task = new FirebaseStorage(_configuration.GetConnectionString("FirebaseBucket"),
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),

                    }
                    ).Child(type).Child(newname).PutAsync(stream);

                task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                try
                {
                    string link = await task;
                    Console.WriteLine(link);
                    return link;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Nepavyko");
                }
            }
            return "";
        }

        private async Task<string> UploadPicture(IFormFile picture, string newName)
        {
            string GDriveKeyPath = Path.Combine(_env.ContentRootPath, "GDriveCredentials.json");
            string DirectoryId = "1I-YrtKmGKyb9Ex-6U_nYwJw-2ynYTo5E";

            // kredencialai google drive api
            var credentials = GoogleCredential.FromFile(GDriveKeyPath).CreateScoped(DriveService.ScopeConstants.Drive);

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials
            });

            var fileMetaData = new Google.Apis.Drive.v3.Data.File()
            {
                Name = newName,
                Parents = new List<string>() { DirectoryId }
            };

            string uploadFileId;

            var request = service.Files.Create(fileMetaData, picture.OpenReadStream(), picture.ContentType);
            request.Fields = "*";
            var results = await request.UploadAsync();

            if (results.Status == Google.Apis.Upload.UploadStatus.Failed)
            {
                return "";
            }
            uploadFileId = request.ResponseBody?.Id;
            Console.WriteLine(uploadFileId);

            return uploadFileId;
        }

        private async Task<string> UploadVideo(IFormFile video, string newName)
        {
            string GDriveKeyPath = Path.Combine(_env.ContentRootPath, "GDriveCredentials.json");
            string DirectoryId = "1pG4kK59LjwRabjSG8083U-DaAPNBg-ew";

            var credentials = GoogleCredential.FromFile(GDriveKeyPath).CreateScoped(DriveService.ScopeConstants.Drive);

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials
            });

            var fileMetaData = new Google.Apis.Drive.v3.Data.File()
            {
                Name = newName,
                Parents = new List<string>() { DirectoryId }
            };

            string uploadFileId;

            var request = service.Files.Create(fileMetaData, video.OpenReadStream(), video.ContentType);
            request.Fields = "*";
            var results = await request.UploadAsync();

            if (results.Status == Google.Apis.Upload.UploadStatus.Failed)
            {
                return "";
            }
            uploadFileId = request.ResponseBody?.Id;
            Console.WriteLine(uploadFileId);
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
            var res = await UploadPicture(picture, "name.png");
             if(res == "")
            {
                return BadRequest("Neikelta");
            }else
            {
                return Ok("ikelta");
            }
        }

        //[HttpGet("getfile/{uri}")]
        //public async Task<ActionResult> DowloadFromGoogleDrive(string uri)
        //{
        //    string GDriveKeyPath = Path.Combine(_env.ContentRootPath, "GDriveCredentials.json");
        //    string DirectoryId = "1pG4kK59LjwRabjSG8083U-DaAPNBg-ew";

        //    var credentials = GoogleCredential.FromFile(GDriveKeyPath).CreateScoped(DriveService.ScopeConstants.Drive);

        //    var service = new DriveService(new BaseClientService.Initializer()
        //    {
        //        HttpClientInitializer = credentials
        //    });

        //    var request = service.Files.Get(uri);
        //    var response = await request.ExecuteAsync();

        //    var fileStream = new FileStream(response.Name, FileMode.Create, FileAccess.Write);



        //    await request.DownloadAsync(fileStream);

        //    return Ok("");

        //}
        [HttpPost]
        [Route("testfirebaseupload")]
        public async Task<ActionResult<string>> UploadToFirebase(IFormFile file)
        {
            Stream stream; /*= picture.OpenReadStream();*/
            if (file.Length > 0)
            {
                //stream = new FileStream(Path.GetFullPath(picture.FileName), FileMode.Open);
                stream = file.OpenReadStream();
                var auth = new FirebaseAuthProvider(new FirebaseConfig(_configuration.GetConnectionString("FirebaseApiKey")));
                var a = await auth.SignInWithEmailAndPasswordAsync(_configuration.GetConnectionString("FirebaseEmail"), _configuration.GetConnectionString("FirebasePass"));

                var cancellation = new CancellationTokenSource();


                var task = new FirebaseStorage(_configuration.GetConnectionString("FirebaseBucket"),
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),

                    }
                    ).Child("videos").Child("naujas.png").PutAsync(stream);

                task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                try
                {
                    string link = await task;
                    Console.WriteLine(link);
                    return Created("Ikelta", link);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Nepavyko");
                }
            }
            //await picture.CopyToAsync(stream).ConfigureAwait(false);
            return BadRequest("Nepavyko");
        }
    }
}
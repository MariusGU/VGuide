using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VirtualGuidePlatform.Data.Entities;
using VirtualGuidePlatform.Data.Entities.Blocks;
using VirtualGuidePlatform.Data.Entities.Dtos;
using VirtualGuidePlatform.Data.Repositories;
using static System.Net.WebRequestMethods;

namespace VirtualGuidePlatform.Controllers
{
    [ApiController]
    [Route("api/guides")]
    public class GuideController : ControllerBase
    {
        private readonly IGuidesRepository guidesRepository;
        private readonly IResponsesRepository _responsesRepository;
        private readonly IBlocksRepository _blocksRepository;
        private readonly IFilesRepository _filesRepository;
        private readonly IAccountsRepository _accountsRepository;


        public GuideController(IGuidesRepository guidesRepository, IResponsesRepository responsesRepository, IBlocksRepository blocksRepository, IFilesRepository filesRepository, IAccountsRepository accountsRepository)
        {
            this.guidesRepository = guidesRepository;
            _responsesRepository = responsesRepository;
            _blocksRepository = blocksRepository;
            _filesRepository = filesRepository;
            _accountsRepository = accountsRepository;
        }
        [HttpPost]
        [RequestSizeLimit(100_000_000)]
        public async Task<ActionResult<Guides>> CreateGuideWithAllData([FromForm] PostGuide guide)
        {
            int imgId = 0;
            int videoId = 0;
            int textId = 0;
            guide.DeserializeBlocks();

            //Console.WriteLine("Title: {0}", guide.Title);
            //Console.WriteLine("Description: {0}", guide.Description);
            //Console.WriteLine("Price: {0}", guide.Price);
            //Console.WriteLine("Language: {0}", guide.Language);

            //======Gido sukurimas=========
            Guides guide1 = new Guides()
            {
                gCreatorId = guide.CreatorId,
                latitude = guide.latitude,
                longtitude = guide.longtitude,
                description = guide.Description,
                city = guide.City,
                name = guide.Title,
                language = guide.Language,
                uDate = DateTime.Now,
                price = guide.Price,
                visible = guide.Visible,
                category = guide.Category
            };
            //==============sukuria nauja gida i duombaze==================
            var created = await guidesRepository.CreateGuide(guide1);
            if(created == null)
            {
                return BadRequest("Guides is not created");
            }
            //====================================
            //----------------------------------
            //======================Isparsina multipartform data==================
            for (int i = 0; i < guide.DeserializedBlocks.Count; i++)
            {
                var block = guide.DeserializedBlocks[i];
                Console.WriteLine("Ieina i bloko paemima");
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
                    //==============================Video bloko=================================
                    case "Video":
                        string[] splitTypeV = guide.Videos[videoId].ContentType.Split('/');
                        string combinedV = created._id + "v" + videoId.ToString() + "." + splitTypeV[1];
                        //------------ikelia video faila i Firebase storage
                        var videoFileID = await _filesRepository.UploadFileToFirebase(guide.Videos[videoId], combinedV, "videos");
                        if (videoFileID == "")
                        {
                            return BadRequest("Nepavyko ikelti video");
                        }
                        //----------------Sukuria video bloka------------------
                        Vblocks videoBlock = new Vblocks
                        {
                            priority = i,
                            URI = videoFileID,
                            FileName = combinedV,
                            contentType = guide.Videos[videoId].ContentType,
                            gId = created._id
                        };
                        //-----------ideda bloka i duomenu baze---------
                        await _blocksRepository.CreateVblock(videoBlock);
                        videoId++;
                        break;
                    //===============================Nuotraukos bloko================================
                    case "Image":
                        Console.WriteLine(guide.Images[imgId].ContentType);
                        string[] splitTypeP = guide.Images[imgId].ContentType.Split('/');
                        Console.WriteLine(splitTypeP[1]);
                        string combinedP = created._id + "p" + imgId.ToString() + "." + splitTypeP[1];
                        //Ikelia faila i Firebase storage
                        var pictureFileID = await _filesRepository.UploadFileToFirebase(guide.Images[imgId], combinedP, "pictures");
                        if (pictureFileID == "")
                        {
                            return BadRequest("Nepavyko ikelti paveikslelio");
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
                        //-------ikelia picture block i duombaze--------------
                        await _blocksRepository.CreatePblock(imageBlock);
                        imgId++;
                        break;
                        //---------------------------------------------------
                }
            }
            return Created("sukurta", guide1);
        }
        //[FromForm] UpdateGuide guide
        [HttpPut]
        [RequestSizeLimit(100_000_000)]
        public async Task<ActionResult<Guides>> UpdateGuideWithData([FromForm] UpdateGuide guide)
        {
            var resguide = await guidesRepository.GetGuide(guide.GuideId);

            var Images = await _blocksRepository.GetPblocks(guide.GuideId);
            var videos = await _blocksRepository.GetVblocks(guide.GuideId);
            var texts = await _blocksRepository.GetTblocks(guide.GuideId);

            int imgId = 0;
            int imgUriId = 0;
            int videoId = 0;
            int videoUriId = 0;
            int textId = 0;
            guide.DeserializeBlocks();

            Console.WriteLine(guide.DeserializedBlocks.Count);

            //======Gido sukurimas=========
            Guides guide1 = new Guides()
            {
                _id = guide.GuideId,
                gCreatorId = guide.CreatorId,
                latitude = resguide.latitude,
                longtitude = resguide.longtitude,
                description = guide.Description,
                city = guide.City,
                name = guide.Title,
                language = guide.Language,
                uDate = DateTime.Now,
                price = guide.Price,
                visible = guide.Visible,
                category = guide.Category
            };
            //==============sukuria nauja gida i duombaze==================
            var created = await guidesRepository.UpdateGuide(guide1);
            if (created == null)
            {
                return BadRequest("Guides is not updated");
            }
            //====================================
            //----------------------------------
            //======================Isparsina multipartform data==================
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
                    //==============================Video bloko=================================
                    case "Video":
                        string[] splitTypeV = guide.Videos[videoId].ContentType.Split('/');
                        string str = DateTime.Now.Ticks.ToString();
                        string nameV = str + "." + splitTypeV[1];
                        //------------ikelia video faila i Firebase storage
                        var videoFileID = await _filesRepository.UploadFileToFirebase(guide.Videos[videoId], nameV, "videos");
                        if (videoFileID == "")
                        {
                            return BadRequest("Nepavyko ikelti video");
                        }
                        //----------------Sukuria video bloka------------------
                        Vblocks videoBlock = new Vblocks
                        {
                            priority = i,
                            URI = videoFileID,
                            FileName = nameV,
                            contentType = guide.Videos[videoId].ContentType,
                            gId = created._id
                        };
                        //-----------ideda bloka i duomenu baze---------
                        await _blocksRepository.CreateVblock(videoBlock);
                        videoId++;
                        break;
                    case "Videouri":
                        var splited = guide.VideosUris[videoUriId].Split('/');
                        var secondSplit = splited[splited.Length - 1].Split('.');
                        var thirdSplit = secondSplit[secondSplit.Length - 1].Split('?');
                        //------------ per nauja ikelia faila is firebase i firebase --------------------
                        string strv1 = DateTime.Now.Ticks.ToString();
                        string newname = strv1 + "." + thirdSplit[0];
                        var videoFileID1 = await _filesRepository.ReuploadFile(guide.VideosUris[videoUriId], newname, "videos");
                        if (videoFileID1 == "")
                        {
                            return BadRequest("Nepavyko ikelti video");
                        }
                        //----------------Sukuria video bloka------------------
                        Vblocks videoBlockV1 = new Vblocks
                        {
                            priority = i,
                            URI = videoFileID1,
                            FileName = newname,
                            contentType = "video/" + thirdSplit[0],
                            gId = created._id
                        };
                        //-----------ideda bloka i duomenu baze---------
                        await _blocksRepository.CreateVblock(videoBlockV1);
                        videoUriId++;
                        break;
                    //===============================Nuotraukos bloko================================
                    case "Image":
                        string[] splitTypeP = guide.Images[imgId].ContentType.Split('/');
                        string str1 = DateTime.Now.Ticks.ToString();
                        string nameP = str1 + "." + splitTypeP[1];
                        Console.WriteLine(nameP);
                        //Ikelia faila i Firebase storage
                        var pictureFileID = await _filesRepository.UploadFileToFirebase(guide.Images[imgId], nameP, "pictures");
                        if (pictureFileID == "")
                        {
                            return BadRequest("Nepavyko ikelti paveikslelio");
                        }
                        //sukuria picture block ikelimui i duombaze
                        Pblocks imageBlock = new Pblocks
                        {
                            priority = i,
                            URI = pictureFileID,
                            FileName = nameP,
                            contentType = guide.Images[imgId].ContentType,
                            gId = created._id
                        };
                        //-------ikelia picture block i duombaze--------------
                        await _blocksRepository.CreatePblock(imageBlock);
                        imgId++;
                        break;

                    case "Imageuri":
                        var splitedP = guide.ImagesUris[imgUriId].Split('/');
                        var secondSplitP = splitedP[splitedP.Length - 1].Split('.');
                        var thirdSplitP = secondSplitP[secondSplitP.Length - 1].Split('?');
                        //Ikelia faila i Firebase storage
                        string strp1 = DateTime.Now.Ticks.ToString();
                        string newnameP = strp1 + "." + thirdSplitP[0];
                        Console.WriteLine(newnameP);
                        var pictureFileIDV1 = await _filesRepository.ReuploadFile(guide.ImagesUris[imgUriId], newnameP, "pictures");
                        if (pictureFileIDV1 == "")
                        {
                            return BadRequest("Nepavyko ikelti paveikslelio");
                        }
                        //sukuria picture block ikelimui i duombaze
                        Pblocks imageBlock1 = new Pblocks
                        {
                            priority = i,
                            URI = pictureFileIDV1,
                            FileName = newnameP,
                            contentType = "image/" + thirdSplitP[0],
                            gId = created._id
                        };
                        //-------ikelia picture block i duombaze--------------
                        await _blocksRepository.CreatePblock(imageBlock1);
                        imgUriId++;
                        break;
                        //---------------------------------------------------
                }
            }
            var deleteres = await DeleteOldGuideInfo(Images, videos, texts);
            return Ok(guide1);
            //return Ok();
        }
        private async Task<bool> DeleteOldGuideInfo(List<Pblocks> images, List<Vblocks> videos, List<Tblocks> texts)
        {
            try
            {
                foreach (Tblocks text in texts)
                {
                    await _blocksRepository.DeleteTBlock(text._id);
                }
                foreach (Pblocks image in images)
                {
                    await _blocksRepository.DeletePBlock(image._id);
                }
                foreach (Vblocks video in videos)
                {
                    await _blocksRepository.DeleteVBlock(video._id);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        private async Task<double> CountRating(string gid)
        {
            var responses = await _responsesRepository.GetResponses(gid);
            var count = 0;
            double sum = 0;
            if(responses.Count < 1)
            {
                return 0;
            }
            else
            {
                foreach (Responses item in responses)
                {
                    sum = sum + item.rating;
                    count++;
                }
                var rating = sum / count;
                int sized = (int)(rating * 10);
                double rounded = (double)(sized) / 10;
                return rounded;
            }
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GuideAllDto>>> GetGuides()
        {
            var guides = await guidesRepository.GetGuides();

            if(guides == null)
            {
                return NotFound();
            }

            if(guides.Count <= 100)
            {
                guides.Sort((x, y) => y.uDate.CompareTo(x.uDate));
            }
            else
            {
                List<Guides> latestGuides = guides.GetRange(0, 100);
                latestGuides.Sort((x, y) => y.uDate.CompareTo(x.uDate));

                foreach(Guides item in latestGuides)
                {
                    Console.WriteLine(item.uDate.ToString());
                }
                guides = latestGuides;
            }

            List<GuideAllDto> guidesToReturn = new List<GuideAllDto>();
            
            foreach (Guides item in guides)
            {
                var pblocks = await _blocksRepository.GetPblocks(item._id);
                pblocks.Sort((x, y) => x.priority.CompareTo(y.priority));
                var path = pblocks[0].URI;

                var rating = await CountRating(item._id);
                var creator = await _accountsRepository.GetCreatorInfoAsync(item.gCreatorId);

                Console.WriteLine("Vidutinis reitingas " + rating.ToString());
                GuideAllDto changed = new GuideAllDto()
                {
                    Image = path,
                    _id = item._id,
                    creatorName = creator.firstname,
                    creatorLastName = creator.lastname,
                    creatorId = item.gCreatorId,
                    latitude = item.latitude,
                    longtitude = item.longtitude,
                    description = item.description,
                    city = item.city,
                    title = item.name,
                    language = item.language,
                    uDate = item.uDate,
                    price = item.price,
                    rating = rating,
                    isFavourite = false,
                    visible = item.visible,
                    category = item.category
                };
                guidesToReturn.Add(changed);
            }
            return Ok(guidesToReturn);
        }
        [HttpGet("{guideId}")]
        public async Task<ActionResult<GuideReturnDto>> GetGuide(string guideId)
        {
            var guide = await guidesRepository.GetGuide(guideId);
            var creator = await _accountsRepository.GetCreatorInfoAsync(guide.gCreatorId);

            var pictures = await _blocksRepository.GetPblocks(guideId);
            var videos = await _blocksRepository.GetVblocks(guideId);
            var texts = await _blocksRepository.GetTblocks(guideId);

            List<BlockDto> blocks = new List<BlockDto>();

            foreach(Pblocks image in pictures)
            {
                BlockDto picture = new BlockDto()
                {
                    Type = "Image",
                    ID = image.priority,
                    pblock = image
                };
                blocks.Add(picture);
            }
            foreach (Vblocks video in videos)
            {
                BlockDto video1 = new BlockDto()
                {
                    Type = "Video",
                    ID = video.priority,
                    vblock = video
                };
                blocks.Add(video1);
            }
            foreach (Tblocks text in texts)
            {
                BlockDto text1 = new BlockDto()
                {
                    Type = "Text",
                    ID = text.priority,
                    tblock = text
                };
                blocks.Add(text1);
            }
            blocks.Sort((x, y) => x.ID.CompareTo(y.ID));
            var rating = await CountRating(guideId);

            GuideReturnDto guideToReturn = new GuideReturnDto()
            {
                _id = guide._id,
                creatorName = creator.firstname,
                creatorLastName = creator.lastname,
                creatorId = guide.gCreatorId,
                description = guide.description,
                latitude = guide.latitude,
                longtitude = guide.longtitude, 
                city = guide.city,
                title = guide.name,
                language = guide.language,
                uDate = guide.uDate,
                price = guide.price,
                rating = rating,
                isFavourite = false,
                visible = guide.visible,
                category = guide.category,
                blocks = blocks
            };
            return Ok(guideToReturn);
        }
        [HttpGet("createdguides/{userid}")]
        public async Task<ActionResult<IEnumerable<GuideAllDto>>> GetUserGuides(string userid)
        {
            var guides = await guidesRepository.GetUserGuides(userid);

            if (guides.Count > 0)
            {
                guides.Sort((x, y) => y.uDate.CompareTo(x.uDate));
            }

            List<GuideAllDto> guidesToReturn = new List<GuideAllDto>();

            foreach (Guides item in guides)
            {
                var pblocks = await _blocksRepository.GetPblocks(item._id);
                pblocks.Sort((x, y) => x.priority.CompareTo(y.priority));
                var path = pblocks[0].URI;

                var rating = await CountRating(item._id);
                var creator = await _accountsRepository.GetCreatorInfoAsync(item.gCreatorId);

                Console.WriteLine("Vidutinis reitingas " + rating.ToString());
                GuideAllDto changed = new GuideAllDto()
                {
                    Image = path,
                    _id = item._id,
                    creatorName = creator.firstname,
                    creatorLastName = creator.lastname,
                    creatorId = item.gCreatorId,
                    latitude = item.latitude,
                    longtitude = item.longtitude,
                    description = item.description,
                    city = item.city,
                    title = item.name,
                    language = item.language,
                    uDate = item.uDate,
                    price = item.price,
                    rating = rating,
                    isFavourite = false,
                    visible = item.visible,
                    category = item.category
                };
                guidesToReturn.Add(changed);
            }
            return Ok(guidesToReturn);
        }


        [HttpGet("creatorguides/{userid}")]
        public async Task<ActionResult<IEnumerable<GuideAllDto>>> GetCreatorGuides(string userid)
        {
            var guides = await guidesRepository.GetCreatorGuides(userid);

            if (guides.Count > 0)
            {
                guides.Sort((x, y) => y.uDate.CompareTo(x.uDate));
            }

            List<GuideAllDto> guidesToReturn = new List<GuideAllDto>();

            foreach (Guides item in guides)
            {
                var pblocks = await _blocksRepository.GetPblocks(item._id);
                pblocks.Sort((x, y) => x.priority.CompareTo(y.priority));
                var path = pblocks[0].URI;

                var rating = await CountRating(item._id);
                var creator = await _accountsRepository.GetCreatorInfoAsync(item.gCreatorId);

                Console.WriteLine("Vidutinis reitingas " + rating.ToString());
                GuideAllDto changed = new GuideAllDto()
                {
                    Image = path,
                    _id = item._id,
                    creatorName = creator.firstname,
                    creatorLastName = creator.lastname,
                    creatorId = item.gCreatorId,
                    latitude = item.latitude,
                    longtitude = item.longtitude,
                    description = item.description,
                    city = item.city,
                    title = item.name,
                    language = item.language,
                    uDate = item.uDate,
                    price = item.price,
                    rating = rating,
                    isFavourite = false,
                    visible = item.visible,
                    category = item.category
                };
                guidesToReturn.Add(changed);
            }
            return Ok(guidesToReturn);
        }

        [HttpGet("savedguides/{userid}")]
        public async Task<ActionResult<IEnumerable<GuideAllDto>>> GetUserSavedGuides(string userid)
        {

            var account = await _accountsRepository.GetAccount(userid);
            if(account == null)
            {
                return NotFound();
            }

            var savedguidesids = account.savedguides;

            List<GuideAllDto> guidesToReturn = new List<GuideAllDto>();

            foreach (string item in savedguidesids)
            {
                var guide = await guidesRepository.GetGuide(item);
                var pblocks = await _blocksRepository.GetPblocks(guide._id);
                pblocks.Sort((x, y) => x.priority.CompareTo(y.priority));
                var path = pblocks[0].URI;

                var rating = await CountRating(guide._id);

                GuideAllDto changed = new GuideAllDto()
                {
                    Image = path,
                    _id = guide._id,
                    creatorName = account.firstname,
                    creatorLastName = account.lastname,
                    creatorId = guide.gCreatorId,
                    latitude = guide.latitude,
                    longtitude = guide.longtitude,
                    description = guide.description,
                    city = guide.city,
                    title = guide.name,
                    language = guide.language,
                    uDate = guide.uDate,
                    price = guide.price,
                    rating = rating,
                    isFavourite = false,
                    visible = guide.visible,
                    category = guide.category
                };
                guidesToReturn.Add(changed);
            }
            return Ok(guidesToReturn);
        }
        [HttpPut("setvisible/{guideid}")]
        public async Task<ActionResult<IEnumerable<GuideAllDto>>> SetGuideVisible(string guideid)
        {

            var guide = await guidesRepository.SetVisible(guideid);
            
            if(guide != null)
            {
                return Ok(guide);
            }

            return NotFound();
        }
        [HttpPut("setinvisible/{guideid}")]
        public async Task<ActionResult<IEnumerable<GuideAllDto>>> SetGuideInvisible(string guideid)
        {
            var count = await _accountsRepository.GetPaydeUsers(guideid);

            if(count > 0)
            {
                return BadRequest("Guide is bought by a least one person");
            }

            var guide = await guidesRepository.SetInvisible(guideid);

            if (guide != null)
            {
                return Ok(guide);
            }

            return NotFound();
        }
        [HttpPost("searched")]
        public async Task<ActionResult<IEnumerable<GuideAllDto>>> GetFiltered(Filters filter)
        {
            var guides = await guidesRepository.GetSearchedGuides(filter);

            if (guides == null || guides.Count <= null)
            {
                return NotFound();
                Console.WriteLine("Ieina cia");
            }

            if (guides.Count <= 100)
            {
                guides.Sort((x, y) => y.uDate.CompareTo(x.uDate));
            }
            else
            {
                List<Guides> latestGuides = guides.GetRange(0, 100);
                latestGuides.Sort((x, y) => y.uDate.CompareTo(x.uDate));

                foreach (Guides item in latestGuides)
                {
                    Console.WriteLine(item.uDate.ToString());
                }
                guides = latestGuides;
            }

            List<GuideAllDto> guidesToReturn = new List<GuideAllDto>();

            foreach (Guides item in guides)
            {
                var pblocks = await _blocksRepository.GetPblocks(item._id);
                pblocks.Sort((x, y) => x.priority.CompareTo(y.priority));
                var path = pblocks[0].URI;

                var rating = await CountRating(item._id);
                var creator = await _accountsRepository.GetCreatorInfoAsync(item.gCreatorId);

                Console.WriteLine("Vidutinis reitingas " + rating.ToString());
                GuideAllDto changed = new GuideAllDto()
                {
                    Image = path,
                    _id = item._id,
                    creatorName = creator.firstname,
                    creatorLastName = creator.lastname,
                    creatorId = item.gCreatorId,
                    latitude = item.latitude,
                    longtitude = item.longtitude,
                    description = item.description,
                    city = item.city,
                    title = item.name,
                    language = item.language,
                    uDate = item.uDate,
                    price = item.price,
                    rating = rating,
                    isFavourite = false,
                    visible = item.visible,
                    category = item.category
                };
                guidesToReturn.Add(changed);
            }
            return Ok(guidesToReturn);
        }

        [HttpDelete("{guideId}")]
        public async Task<ActionResult> DeleteGuide(string guideId)
        {
            var guide = await guidesRepository.GetGuide(guideId);

            if(guide == null)
            {
                return BadRequest("");
            }

            var isDeleted = await guidesRepository.DeleteGuide(guideId);

            if (!isDeleted)
            {
                return BadRequest("");
            }

            var pictures = await _blocksRepository.GetPblocks(guideId);
            var videos = await _blocksRepository.GetVblocks(guideId);
            var texts = await _blocksRepository.GetTblocks(guideId);
            var responses = await _responsesRepository.GetResponses(guideId);

            foreach(Pblocks picture in pictures)
            {
                var resP = await _blocksRepository.DeletePBlock(picture._id);
                var resFP = await _filesRepository.DeleteFile("pictures/" + picture.FileName);
            }

            foreach (Vblocks video in videos)
            {
                var resV = await _blocksRepository.DeleteVBlock(video._id);
                var resFV = await _filesRepository.DeleteFile("videos/" + video.FileName);
            }

            foreach(Tblocks text in texts)
            {
                var resT = await _blocksRepository.DeleteTBlock(text._id);
            }

            foreach(Responses response in responses)
            {
                var resR = await _responsesRepository.DeleteResponse(response._id);
            }

            return Ok("Deleted");
        }

        [HttpDelete("deletefile")]
        public async Task<ActionResult<bool>> DeleteFile([FromBody] string path)
        {
            var res = await _filesRepository.DeleteFile(path);

            if(res == false)
            {
                return BadRequest("");
            }

            return res;
        }
        [HttpGet("getfile")]
        public async Task<ActionResult<bool>> FileDownload([FromBody] string path)
        {
            var res = await _filesRepository.DownloadFile(path);

            if(res == "")
            {
                return NotFound(res);
            }

            return Ok(res);
        }
        [HttpPost("uploadfile")]
        public async Task<ActionResult<string>> ReUploadFile([FromBody] string path)
        {
            var splited = path.Split('/');
            var secondSplit = splited[splited.Length - 1].Split('.');
            var thirdSplit = secondSplit[secondSplit.Length - 1].Split('?');

            Console.WriteLine();
            var res = await _filesRepository.ReuploadFile(path, "temp1.jpeg", "pictures");

            if (res == "")
            {
                Console.WriteLine("ieina");
                return BadRequest("");
            }

            return res;
        }
    }
}

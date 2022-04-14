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
                locationXY = guide.LocationXY,
                description = guide.Description,
                city = guide.City,
                name = guide.Title,
                language = guide.Language,
                uDate = DateTime.Now,
                price = guide.Price,
                visible = guide.Visible
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
                    description = item.description,
                    city = item.city,
                    title = item.name,
                    language = item.language,
                    uDate = item.uDate,
                    price = item.price,
                    rating = rating,
                    isFavourite = false,
                    visible = item.visible
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
                city = guide.city,
                title = guide.name,
                language = guide.language,
                uDate = guide.uDate,
                price = guide.price,
                rating = rating,
                isFavourite = false,
                visible = guide.visible,
                blocks = blocks
            };
            return Ok(guideToReturn);
        }
    }
}

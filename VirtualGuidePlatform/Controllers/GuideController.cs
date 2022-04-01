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

        public GuideController(IGuidesRepository guidesRepository, IResponsesRepository responsesRepository, IBlocksRepository blocksRepository, IGuidesRepository guidesRepository1)
        {
            this.guidesRepository = guidesRepository;
            _responsesRepository = responsesRepository;
            _blocksRepository = blocksRepository;
        }

        [HttpPost]
        public async Task<ActionResult<Guides>> CreateGuide(Guides guide)
        {
            await guidesRepository.CreateGuide(guide);

            return Created("Sukurta", guide);
        }
        //[HttpGet("{guideId}")]
        //public async Task<ActionResult<Guides>> GetGuide(string guideId)
        //{
        //    var guide = await guidesRepository.GetGuide(guideId);

        //    return Ok(guide);
        //}
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
                List<Guides> latestGuides = guides.GetRange(1, 2);
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
                //var path = "C:\\Users\\Marius\\Desktop\\Guide\\VirtualGuidePlatform\\VirtualGuidePlatform\\Images\\624575a2075fa8cc9616271cp0.jpeg";
                var bytes = await System.IO.File.ReadAllBytesAsync(path);
                FileContentResult file = File(bytes, pblocks[0].contentType, Path.GetFileName(path));

                var rating = await CountRating(item._id);
                Console.WriteLine("Vidutinis reitingas " + rating.ToString());
                GuideAllDto changed = new GuideAllDto()
                {
                    Image = file,
                    _id = item._id,
                    creatorName = "Vardas",
                    creatorLastName = "Pavarde",
                    creatorId = item.gCreatorId.ToString(),
                    description = item.description,
                    city = item.city,
                    title = item.name,
                    language = item.language,
                    uDate = item.uDate,
                    price = item.price,
                    rating = rating,
                    isFavourite = false
                };
                guidesToReturn.Add(changed);
            }
            return Ok(guidesToReturn);
        }
        [HttpGet("{guideId}")]
        public async Task<ActionResult<GuideReturnDto>> GetGuide(string guideId)
        {
            var guide = await guidesRepository.GetGuide(guideId);

            var pictures = await _blocksRepository.GetPblocks(guideId);
            var videos = await _blocksRepository.GetVblocks(guideId);
            var texts = await _blocksRepository.GetTblocks(guideId);

            List<BlockDto> blocks = new List<BlockDto>();

            foreach(Pblocks image in pictures)
            {
                var path = image.URI;
                var bytes = await System.IO.File.ReadAllBytesAsync(path);
                FileContentResult pfile = File(bytes, image.contentType, Path.GetFileName(path));

                BlockDto picture = new BlockDto()
                {
                    Type = "Image",
                    ID = image.priority,
                    image = pfile,
                    pblock = image
                };
                blocks.Add(picture);
            }
            foreach (Vblocks video in videos)
            {
                var path = video.URI;
                var bytes = await System.IO.File.ReadAllBytesAsync(path);
                FileContentResult vfile = File(bytes, video.contentType, Path.GetFileName(path));

                BlockDto video1 = new BlockDto()
                {
                    Type = "Video",
                    ID = video.priority,
                    video = vfile,
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
            Console.WriteLine(blocks.ElementAt(0).ID);
            var rating = await CountRating(guideId);
            GuideReturnDto guideToReturn = new GuideReturnDto()
            {
                _id = guide._id,
                creatorName = "Name",
                creatorLastName = "LastName",
                creatorId = guide.gCreatorId.ToString(),
                description = guide.description,
                city = guide.city,
                title = guide.name,
                language = guide.language,
                uDate = guide.uDate,
                price = guide.price,
                rating = rating,
                isFavourite = false,
                blocks = blocks
            };

            return Ok(guideToReturn);
        }
    }
}

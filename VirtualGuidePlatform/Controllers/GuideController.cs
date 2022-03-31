using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VirtualGuidePlatform.Data.Entities;
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

        public GuideController(IGuidesRepository guidesRepository, IResponsesRepository responsesRepository)
        {
            this.guidesRepository = guidesRepository;
            _responsesRepository = responsesRepository;
        }

        [HttpPost]
        public async Task<ActionResult<Guides>> CreateGuide(Guides guide)
        {
            await guidesRepository.CreateGuide(guide);

            return Created("Sukurta", guide);
        }
        [HttpGet("{guideId}")]
        public async Task<ActionResult<Guides>> GetGuide(string guideId)
        {
            var guide = await guidesRepository.GetGuide(guideId);

            return Ok(guide);
        }
        private async Task<double> CountRating(string gid)
        {
            var responses = await _responsesRepository.GetResponses(gid);
            var count = 0;
            double sum = 0;
            foreach(Responses item in responses)
            {
                sum = sum + item.rating;
                count++;
            }
            if(sum/count == 0)
            {
                return 0;
            }else
            {
                return sum / count;
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
                var path = "C:\\Users\\Marius\\Desktop\\Guide\\VirtualGuidePlatform\\VirtualGuidePlatform\\Images\\624575a2075fa8cc9616271cp0.jpeg";
                var bytes = await System.IO.File.ReadAllBytesAsync(path);
                FileContentResult file = File(bytes, "text/plain", Path.GetFileName(path));

                var rating = await CountRating(item._id);
                int sized = (int)(rating * 10);
                double rounded = (double)(sized) / 10;
                Console.WriteLine("Vidutinis reitingas " + rounded.ToString());
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
                    rating = rounded,
                    isFavourite = false
                };
                guidesToReturn.Add(changed);
            }
            return Ok(guidesToReturn);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualGuidePlatform.Data.Entities;
using VirtualGuidePlatform.Data.Repositories;

namespace VirtualGuidePlatform.Controllers
{
    [ApiController]
    [Route("api/guides")]
    public class GuideController : ControllerBase
    {
        private readonly IGuidesRepository guidesRepository;

        public GuideController(IGuidesRepository guidesRepository)
        {
            this.guidesRepository = guidesRepository;
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
    }
}

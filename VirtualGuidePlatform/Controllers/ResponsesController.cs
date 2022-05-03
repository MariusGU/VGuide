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
    [Route("api/{gid}/responses")]
    public class ResponsesController : ControllerBase
    {
        private readonly IResponsesRepository _responsesRepository;
        public ResponsesController(IResponsesRepository responsesRepository)
        {
            _responsesRepository = responsesRepository;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Responses>>> GetAll(string gid)
        {
            var all = await _responsesRepository.GetResponses(gid);

            if(all.Count == 0)
            {
                Console.WriteLine("Ieina");
                return NotFound(null);
            }
            //var elems = JsonConvert.SerializeObject(all);

            return Ok(all);
        }
        [HttpPost]
        public async Task<ActionResult<Responses>> CreateOne(Responses response)
        {
            await _responsesRepository.CreateResponse(response);

            return Created("sukurta", response);
        }
        [HttpGet("{userId}")]
        public async Task<ActionResult<Responses>> GetUserResponse(string userId, string gid)
        {
            var response = await _responsesRepository.GetUserResponse(userId, gid);

            if(response == null)
            {
                return NotFound(null);
            }
            return Ok(response);
        }
    }
}

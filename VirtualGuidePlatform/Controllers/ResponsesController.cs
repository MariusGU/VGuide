using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualGuidePlatform.Data.Entities;
using VirtualGuidePlatform.Data.Entities.Dtos;
using VirtualGuidePlatform.Data.Repositories;

namespace VirtualGuidePlatform.Controllers
{
    [ApiController]
    [Route("api/responses")]
    public class ResponsesController : ControllerBase
    {
        private readonly IResponsesRepository _responsesRepository;
        private readonly IAccountsRepository _accountsRepository;

        public ResponsesController(IResponsesRepository responsesRepository, IAccountsRepository accountsRepository)
        {
            _responsesRepository = responsesRepository;
            _accountsRepository = accountsRepository;
        }
        [HttpGet("{gid}")]
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
        [HttpGet("{gid}/userresponse/{userId}")]
        public async Task<ActionResult<Responses>> GetUserResponse(string userId, string gid)
        {
            var response = await _responsesRepository.GetUserResponse(userId, gid);

            if(response == null)
            {
                return NotFound(null);
            }
            return Ok(response);
        }
        [HttpGet("{gid}/notuserresponses/{userId}")]
        public async Task<ActionResult<List<ResponseReturnDto>>> GetNotUserResponse(string userId, string gid)
        {
            var response = await _responsesRepository.GetNotUserResponse(userId, gid);

            if (response.Count() == 0 || response == null)
            {
                return NotFound(null);
            }

            List<ResponseReturnDto> list = new List<ResponseReturnDto>();
            foreach (Responses item in response)
            {
                Accounts acc = await _accountsRepository.GetAccount(item.uId);
                ResponseReturnDto responseReturn = new ResponseReturnDto(item, acc.firstname, acc.lastname, acc.ppicture);
                list.Add(responseReturn);
            }

            return Ok(list);
        }
    }
}

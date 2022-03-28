using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualGuidePlatform.Data.Entities;
using VirtualGuidePlatform.Data.Repositories;

namespace VirtualGuidePlatform.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountController : ControllerBase
    {

        private readonly IAccountsRepository _accountsRepository;
        public AccountController(IAccountsRepository accountsRepository)
        {
            _accountsRepository = accountsRepository;
        }

        [HttpGet("{id}")]
        public async Task<string> GetOne(string id)
        {
            var account = await _accountsRepository.GetAccount(id);
            var json = JsonConvert.SerializeObject(account);
            return json;
        }

        [HttpGet]
        public JsonResult GetAll()
        {
            var all = _accountsRepository.GetAccounts();
            return new JsonResult(all);

        }
        [HttpPost]
        public JsonResult CreateOne(Accounts account)
        {
            _accountsRepository.CreateAccount(account);

            return new JsonResult("sukurta");
        }
    }
}
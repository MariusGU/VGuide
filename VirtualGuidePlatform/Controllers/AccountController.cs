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
        public async Task<Accounts> GetOne(string id)
        {
            var account = await _accountsRepository.GetAccount(id);
            return account;
        }

        [HttpGet]
        public async Task<IEnumerable<Accounts>> GetAll()
        {
            var all = await _accountsRepository.GetAccounts();
            //var elems = JsonConvert.SerializeObject(all);

            return all;
        }
        [HttpPost]
        public async Task<ActionResult<Accounts>> CreateOne(Accounts account)
        {
            await _accountsRepository.CreateAccount(account);

            return Created("sukurta", account);
        }
    }
}
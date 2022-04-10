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
        public async Task<ActionResult<AccountsDto>> Login(Login login)
        {
            var obj = await _accountsRepository.Login(login.email, login.password);
            if(obj != null)
            {
                AccountsDto acc = new AccountsDto()
                {
                    _id = obj._id,
                    email = obj.email,
                    languages = obj.languages,
                    followers = obj.followers,
                    followed = obj.followed,
                    ppicture = obj.ppicture,
                    savedguides = obj.savedguides,
                    payedguides = obj.payedguides
                };
                return Ok(acc);
            }
            else
            {
                return NotFound("User not found, check your email or password");
            }
        }
        [HttpGet]
        public async Task<IEnumerable<Accounts>> GetAll()
        {
            var all = await _accountsRepository.GetAccounts();
            //var elems = JsonConvert.SerializeObject(all);

            return all;
        }
        [HttpPost]
        public async Task<ActionResult<AccountsDto>> CreateOne(Accounts account)
        {
            await _accountsRepository.CreateAccount(account);
            AccountsDto acc = new AccountsDto() 
            {
                _id = account._id,
                email = account.email,
                languages = account.languages,
                followers = account.followers,
                followed = account.followed,
                ppicture = account.ppicture,
                savedguides = account.savedguides,
                payedguides = account.payedguides
            };
            return Created("sukurta", acc);
        }
        [HttpPut("{userId}")]
        public async Task<ActionResult<AccountsDto>> UpdateAccount(Accounts account, string userId)
        {
            var accountUpdated = await _accountsRepository.UpdateAccount(account, userId);

            if(accountUpdated == null)
            {
                return BadRequest("Nepavyko pakeisti");
            }

            return Ok(accountUpdated);
        }
        [HttpPut("follow/{userId}")]
        public async Task<ActionResult<AccountsDto>> UpdateFollow(Accounts account, string userId)
        {
            var accountUpdated = await _accountsRepository.UpdateFollow(account, userId);

            if (accountUpdated == null)
            {
                return BadRequest("Nepavyko pakeisti");
            }

            return Ok(accountUpdated);
        }
        [HttpPut("unfollow/{userId}")]
        public async Task<ActionResult<AccountsDto>> UpdateUnfollow(Accounts account, string userId)
        {
            var accountUpdated = await _accountsRepository.UpdateUnfollow(account, userId);

            if (accountUpdated == null)
            {
                return BadRequest("Nepavyko pakeisti");
            }

            return Ok(accountUpdated);
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualGuidePlatform.Data.Entities;
using VirtualGuidePlatform.Data.Entities.Dtos.AccountDtos;
using VirtualGuidePlatform.Data.Repositories;

namespace VirtualGuidePlatform.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly IFilesRepository _filesRepository;
        public AccountController(IAccountsRepository accountsRepository, IFilesRepository filesRepository)
        {
            _accountsRepository = accountsRepository;
            _filesRepository = filesRepository;
        }
        [HttpGet("{creatorId}")]
        public async Task<ActionResult<AccountDtoCreator>> GetCreator(string creatorId)
        {
            var creator = await _accountsRepository.GetCreatorInfoAsync(creatorId);

            if(creator == null)
            {
                return NotFound("Creator was not found");
            }
            return Ok(creator);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AccountsDto>> Login(Login login)
        {
            var obj = await _accountsRepository.Login(login.email, login.password);
            if (obj != null)
            {
                AccountsDto acc = new AccountsDto()
                {
                    _id = obj._id,
                    firstname = obj.firstname,
                    lastname = obj.lastname,
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
        [HttpPost("uploadphoto/{userId}")]
        public async Task<ActionResult<AccountsDto>> UploadProfilePicture([FromForm] UploadFile file, string userId)
        {
            var obj = await _accountsRepository.GetAccount(userId);
            if (obj != null)
            {
                string[] type = file.file.ContentType.Split('/');
                var resFile = await _filesRepository.UploadFileToFirebase(file.file, obj._id + "." + type[1], "profilepictures");
                if(resFile == "")
                {
                    return BadRequest("");
                }

                obj.ppicture = resFile;

                var resacc = await _accountsRepository.UpdateAccount(obj, userId);

                if(resacc == null)
                {
                    return BadRequest("");
                }

                AccountsDto acc = new AccountsDto()
                {
                    _id = obj._id,
                    firstname = obj.firstname,
                    lastname = obj.lastname,
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
                return NotFound("User not found");
            }
        }
        [HttpPost("register")]
        public async Task<ActionResult<AccountsDto>> CreateOne(Accounts account)
        {
            account.languages = new string[0];
            account.followers = new string[0];
            account.followed = new string[0];
            account.ppicture = "";
            account.savedguides = new string[0];
            account.payedguides = new string[0];
            await _accountsRepository.CreateAccount(account);
            AccountsDto acc = new AccountsDto()
            {
                _id = account._id,
                firstname = account.firstname,
                lastname = account.lastname,
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

            if (accountUpdated == null)
            {
                return BadRequest("Nepavyko pakeisti");
            }

            return Ok(accountUpdated);
        }
        [HttpPut("follow/{userId}")]
        public async Task<ActionResult<AccountsDto>> UpdateFollow([FromBody] string creatorID, string userId)
        {
            var accountUpdated = await _accountsRepository.UpdateFollow(creatorID, userId);

            if (accountUpdated == null)
            {
                return BadRequest("Nepavyko pakeisti");
            }

            return Ok(accountUpdated);
        }
        [HttpPut("unfollow/{userId}")]
        public async Task<ActionResult<AccountsDto>> UpdateUnfollow([FromBody] string creatorID, string userId)
        {
            var accountUpdated = await _accountsRepository.UpdateUnfollow(creatorID, userId);

            if (accountUpdated == null)
            {
                return BadRequest("Nepavyko pakeisti");
            }

            return Ok(accountUpdated);
        }
        [HttpPut("saveguide/{userId}")]
        public async Task<ActionResult<AccountsDto>> UpdateAddSaved([FromBody] string guideID, string userId)
        {
            var accountUpdated = await _accountsRepository.UpdateAddSaved(guideID, userId);

            if (accountUpdated == null)
            {
                return BadRequest("Nepavyko pakeisti");
            }

            return Ok(accountUpdated);
        }
        [HttpPut("removesavedguide/{userId}")]
        public async Task<ActionResult<AccountsDto>> UpdateRemoveSaved([FromBody] string guideID, string userId)
        {
            var accountUpdated = await _accountsRepository.UpdateRemoveSaved(guideID, userId);

            if (accountUpdated == null)
            {
                return BadRequest("Nepavyko pakeisti");
            }

            return Ok(accountUpdated);
        }
        [HttpPut("addpayed/{userId}")]
        public async Task<ActionResult<AccountsDto>> UpdateAddPayed([FromBody] string guideID, string userId)
        {
            Console.WriteLine(guideID);
            var accountUpdated = await _accountsRepository.UpdateAddPayed(guideID, userId);

            if (accountUpdated == null)
            {
                return BadRequest("Nepavyko");
            }

            return Ok(accountUpdated);
        }
        [HttpPut("changepassword/{userId}")]
        public async Task<ActionResult<AccountsDto>> ChangePassword(AccountPswChange passwordData, string userId)
        {
            var accountUpdated = await _accountsRepository.ChangePassword(passwordData, userId);

            if (accountUpdated == null)
            {
                return BadRequest("Old password is incorrect");
            }

            return Ok(accountUpdated);
        }


        [HttpGet("followers/{userId}")]
        public async Task<ActionResult<List<Accounts>>> GetFollowersList(string userId)
        {
            try
            {
                var results = await _accountsRepository.GetFollowers(userId);
                return Ok(results);
            }
            catch
            {
                return NoContent();
            }
        }

        [HttpGet("following/{userId}")]
        public async Task<ActionResult<List<Accounts>>> GetFollowedPeopleList(string userId)
        {
            try
            {
                var results = await _accountsRepository.GetFollowing(userId);
                return Ok(results);
            }
            catch
            {
                return NoContent();
            }
        }
    }
}
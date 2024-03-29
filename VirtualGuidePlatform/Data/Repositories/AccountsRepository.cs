﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualGuidePlatform.Data.Entities;
using VirtualGuidePlatform.Data.Entities.Dtos.AccountDtos;

namespace VirtualGuidePlatform.Data.Repositories
{
    public interface IAccountsRepository
    {
        Task<Accounts> CreateAccount(Accounts account);
        Task<Accounts> GetAccount(string id);
        Task<List<Accounts>> GetAccounts();
        Task<Accounts> Login(string email, string password);
        Task<AccountsDto> UpdateAccount(Accounts account, string id);
        Task<AccountsDto> UpdateFollow(string creatorID, string userId);
        Task<AccountsDto> UpdateUnfollow(string creatorID, string userId);
        Task<AccountsDto> UpdateAddSaved(string guideID, string userId);
        Task<AccountsDto> UpdateRemoveSaved(string guideID, string userId);
        Task<AccountDtoCreator> GetCreatorInfoAsync(string creatorId);
        Task<AccountsDto> UpdateAddPayed(string guideID, string userId);
        Task<AccountsDto> ChangePassword(AccountPswChange passwordData, string userId);
        Task<List<AccountDtoCreator>> GetFollowers(string userId);
        Task<List<AccountDtoCreator>> GetFollowing(string userId);
        Task<int> GetPaydeUsers(string guideId);
    }

    public class AccountsRepository : IAccountsRepository
    {
        private MongoClient _mongoClient;
        private IMongoDatabase _database;
        private IMongoCollection<Accounts> _accountTable;
        private IConfiguration _configuration;

        public AccountsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _mongoClient = new MongoClient(_configuration.GetConnectionString("VVGuideConnection"));
            _database = _mongoClient.GetDatabase("app");
            _accountTable = _database.GetCollection<Accounts>("accounts");
        }
        public async Task<List<Accounts>> GetAccounts()
        {
            return await _accountTable.AsQueryable().ToListAsync();
        }

        public async Task<Accounts> GetAccount(string id)
        {
            //var obj = await _accountTable.FindAsync(x => x._id == ObjectId.Parse(id));
            var obj = await _accountTable.FindAsync(x => x._id == id);
            return obj.FirstOrDefault();
        }
        public async Task<Accounts> Login(string email, string password)
        {
            //var obj = await _accountTable.FindAsync(x => x._id == ObjectId.Parse(id));
            var obj = await _accountTable.FindAsync(x => x.email == email && x.password == password);
            return obj.FirstOrDefault();
        }
        public async Task<Accounts> CreateAccount(Accounts account)
        {
            var obj = _accountTable.Find(x => x._id == account._id).FirstOrDefault();
            if (obj == null)
            {
                await _accountTable.InsertOneAsync(account);
            }
            else
            {
                await _accountTable.ReplaceOneAsync(x => x._id == account._id, account);
            }
            return account;
        }
        private Accounts MapAccountUpdate(Accounts account, Accounts original)
        {
            Accounts mapped = original;
            if (account._id != null)
            {
                mapped._id = account._id;
            }
            if (account.firstname != null)
            {
                mapped.firstname = account.firstname;
            }
            if (account.lastname != null)
            {
                mapped.lastname = account.lastname;
            }
            if (account.email != null)
            {
                mapped.email = account.email;
            }
            if (account.password != null)
            {
                mapped.password = account.password;
            }
            if (account.languages != null)
            {
                mapped.languages = account.languages;
            }
            if (account.followers != null)
            {
                mapped.followers = account.followers;
            }
            if (account.followed != null)
            {
                mapped.followed = account.followed;
            }
            if (account.ppicture != null)
            {
                mapped.ppicture = account.ppicture;
            }
            if (account.savedguides != null)
            {
                mapped.savedguides = account.savedguides;
            }
            if (account.payedguides != null)
            {
                mapped.payedguides = account.payedguides;
            }
            return mapped;
        }
        public async Task<AccountsDto> UpdateAccount(Accounts account, string id)
        {
            var obj = (await _accountTable.FindAsync(x => x._id == id)).FirstOrDefault();
            if (obj == null)
            {
                return null;
            }
            var mapped = MapAccountUpdate(account, obj);

            var acc = await _accountTable.ReplaceOneAsync(x => x._id == id, mapped);

            if (acc.IsAcknowledged)
            {
                return new AccountsDto(mapped._id, mapped.firstname, mapped.lastname, mapped.email, mapped.languages,
                    mapped.followers, mapped.followed, mapped.ppicture, mapped.savedguides, mapped.payedguides);
            }

            return null;
        }
        public async Task<AccountsDto> UpdateFollow(string creatorID, string userId)
        {
            var useraccount = (await _accountTable.FindAsync(x => x._id == userId)).FirstOrDefault();
            var creatoraccount = (await _accountTable.FindAsync(x => x._id == creatorID)).FirstOrDefault();
            if (useraccount == null)
            {
                return null;
            }

            var userfollowed = AddToArray(creatorID, useraccount.followed);
            useraccount.followed = userfollowed;
            var acc = await _accountTable.ReplaceOneAsync(x => x._id == userId, useraccount);

            var creatorfollowed = AddToArray(userId, creatoraccount.followers);
            creatoraccount.followers = creatorfollowed;
            var cacc = await _accountTable.ReplaceOneAsync(x => x._id == creatorID, creatoraccount);

            var mapped = useraccount;

            if (acc.IsAcknowledged && cacc.IsAcknowledged)
            {
                return new AccountsDto(mapped._id, mapped.firstname, mapped.lastname, mapped.email, mapped.languages,
                    mapped.followers, mapped.followed, mapped.ppicture, mapped.savedguides, mapped.payedguides);
            }

            return null;
        }
        private string[] RemoveFromArray(string toremove, string[] removefrom)
        {
            int place = 0;
            if (removefrom.Contains(toremove))
            {
                string[] removed = new string[removefrom.Length - 1];
                for (int i = 0; i < removefrom.Length; i++)
                {
                    if (removefrom[i] != toremove)
                    {
                        removed[place] = removefrom[i];
                        place++;
                    }
                }
                return removed;
            }
            return removefrom;
        }
        private string[] AddToArray(string toadd, string[] intoAdd)
        {
            string[] added = new string[intoAdd.Length + 1];
            if (!intoAdd.Contains(toadd))
            {
                for (int i = 0; i < intoAdd.Length; i++)
                {
                    added[i] = intoAdd[i];
                }
                added[intoAdd.Length] = toadd;
                return added;
            }
            Console.WriteLine(intoAdd.Length);

            return intoAdd;
        }
        public async Task<AccountsDto> UpdateUnfollow(string creatorID, string userId)
        {
            var useraccount = (await _accountTable.FindAsync(x => x._id == userId)).FirstOrDefault();
            var creatoraccount = (await _accountTable.FindAsync(x => x._id == creatorID)).FirstOrDefault();
            if (useraccount == null)
            {
                return null;
            }

            var userunfollowed = RemoveFromArray(creatorID, useraccount.followed);
            useraccount.followed = userunfollowed;
            var acc = await _accountTable.ReplaceOneAsync(x => x._id == userId, useraccount);

            var creatorunfollowed = RemoveFromArray(userId, creatoraccount.followers);
            creatoraccount.followers = creatorunfollowed;
            var cacc = await _accountTable.ReplaceOneAsync(x => x._id == creatorID, creatoraccount);

            var mapped = useraccount;

            if (acc.IsAcknowledged && cacc.IsAcknowledged)
            {
                return new AccountsDto(mapped._id, mapped.firstname, mapped.lastname, mapped.email, mapped.languages,
                    mapped.followers, mapped.followed, mapped.ppicture, mapped.savedguides, mapped.payedguides);
            }

            return null;
        }
        public async Task<AccountsDto> UpdateAddSaved(string guideID, string userId)
        {
            var useraccount = (await _accountTable.FindAsync(x => x._id == userId)).FirstOrDefault();
            if (useraccount == null)
            {
                return null;
            }

            var savedGuides = AddToArray(guideID, useraccount.savedguides);
            useraccount.savedguides = savedGuides;
            var acc = await _accountTable.ReplaceOneAsync(x => x._id == userId, useraccount);

            var mapped = useraccount;

            if (acc.IsAcknowledged)
            {
                return new AccountsDto(mapped._id, mapped.firstname, mapped.lastname, mapped.email, mapped.languages,
                    mapped.followers, mapped.followed, mapped.ppicture, mapped.savedguides, mapped.payedguides);
            }
            return null;
        }
        public async Task<AccountsDto> UpdateRemoveSaved(string guideID, string userId)
        {
            var useraccount = (await _accountTable.FindAsync(x => x._id == userId)).FirstOrDefault();
            if (useraccount == null)
            {
                return null;
            }

            var savedGuides = RemoveFromArray(guideID, useraccount.savedguides);
            useraccount.savedguides = savedGuides;
            var acc = await _accountTable.ReplaceOneAsync(x => x._id == userId, useraccount);

            var mapped = useraccount;

            if (acc.IsAcknowledged)
            {
                return new AccountsDto(mapped._id, mapped.firstname, mapped.lastname, mapped.email, mapped.languages,
                    mapped.followers, mapped.followed, mapped.ppicture, mapped.savedguides, mapped.payedguides);
            }
            return null;
        }
        public async Task<AccountsDto> UpdateAddPayed(string guideID, string userId)
        {
            Console.WriteLine("ieina");
            var useraccount = (await _accountTable.FindAsync(x => x._id == userId)).FirstOrDefault();
            if (useraccount == null)
            {
                return null;
            }

            var payedGuides = AddToArray(guideID, useraccount.payedguides);
            useraccount.payedguides = payedGuides;
            var acc = await _accountTable.ReplaceOneAsync(x => x._id == userId, useraccount);

            var mapped = useraccount;

            if (acc.IsAcknowledged)
            {
                return new AccountsDto(mapped._id, mapped.firstname, mapped.lastname, mapped.email, mapped.languages,
                    mapped.followers, mapped.followed, mapped.ppicture, mapped.savedguides, mapped.payedguides);
            }
            return null;
        }
        public async Task<AccountDtoCreator> GetCreatorInfoAsync(string creatorId)
        {
            var creator = (await _accountTable.FindAsync(x => x._id == creatorId)).FirstOrDefault();
            if(creator == null)
            {
                return null;
            }

            Console.WriteLine("ieina");

            AccountDtoCreator creatorReturn = new AccountDtoCreator(creator._id, creator.firstname, creator.lastname, 
                creator.ppicture, creator.followers, creator.followed);

            return creatorReturn;
        }
        public async Task<AccountsDto> ChangePassword(AccountPswChange passwordData, string userId)
        {
            Console.WriteLine("ieina");
            var useraccount = (await _accountTable.FindAsync(x => x._id == userId && x.password == passwordData.OldPassword)).FirstOrDefault();
            if (useraccount == null)
            {
                return null;
            }

            useraccount.password = passwordData.NewPassword;
            var acc = await _accountTable.ReplaceOneAsync(x => x._id == userId, useraccount);

            var mapped = useraccount;

            if (acc.IsAcknowledged)
            {
                return new AccountsDto(mapped._id, mapped.firstname, mapped.lastname, mapped.email, mapped.languages,
                    mapped.followers, mapped.followed, mapped.ppicture, mapped.savedguides, mapped.payedguides);
            }
            return null;
        }



        // Get User's followers list
        public async Task<List<AccountDtoCreator>> GetFollowers(string userId)
        {
            try
            {
                // Go through accounts and check if given user is in the list of followed people
                var accounts = (await _accountTable.FindAsync(x => x.followed.Contains(userId))).ToList();
               
                var dtoAccounts = new List<AccountDtoCreator>();

                foreach(var account in accounts)
                {
                    dtoAccounts.Add(new AccountDtoCreator(account));
                }

                return dtoAccounts;
            }
            catch
            {
                return new List<AccountDtoCreator>();
            }

        }

        // Get User's followed people list
        public async Task<List<AccountDtoCreator>> GetFollowing(string userId)
        {
            try
            {
                // Go through accounts and check if given user is in the followers list
                var accounts = (await _accountTable.FindAsync(x => x.followers.Contains(userId))).ToList();

                var dtoAccounts = new List<AccountDtoCreator>();

                foreach (var account in accounts)
                {
                    dtoAccounts.Add(new AccountDtoCreator(account));
                }

                return dtoAccounts;
            }
            catch
            {
                return new List<AccountDtoCreator>();
            }

        }
        public async Task<int> GetPaydeUsers(string guideId)
        {
            var count = (await _accountTable.FindAsync(x => x.payedguides.Contains(guideId))).ToList();

            if(count.Count == 0)
            {
                return 0;
            }
            else
            {
                return count.Count;
            }
        }
    }
}
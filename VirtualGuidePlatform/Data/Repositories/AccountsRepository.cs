using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualGuidePlatform.Data.Entities;

namespace VirtualGuidePlatform.Data.Repositories
{
    public interface IAccountsRepository
    {
        Task<Accounts> CreateAccount(Accounts account);
        Task<Accounts> GetAccount(string id);
        Task<List<Accounts>> GetAccounts();
        Task<Accounts> Login(string email, string password);
        Task<AccountsDto> UpdateAccount(Accounts account, string id);
        Task<AccountsDto> UpdateFollow(Accounts account, string userId);
        Task<AccountsDto> UpdateUnfollow(Accounts account, string userId);
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
            if(account._id != null)
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
            if(obj == null)
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
        public async Task<AccountsDto> UpdateFollow(Accounts account, string userId)
        {
            var useraccount = (await _accountTable.FindAsync(x => x._id == userId)).FirstOrDefault();
            var creatoraccount = (await _accountTable.FindAsync(x => x._id == account.followed[0])).FirstOrDefault();
            if (useraccount == null)
            {
                return null;
            }

            var userfollowed = AddToArray(account.followed[0], useraccount.followed);
            useraccount.followed = userfollowed;
            var acc = await _accountTable.ReplaceOneAsync(x => x._id == userId, useraccount);

            var creatorfollowed = AddToArray( account.followed[0], creatoraccount.followers);
            creatoraccount.followers = creatorfollowed;
            var cacc = await _accountTable.ReplaceOneAsync(x => x._id == account.followed[0], creatoraccount);

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
            string[] added = new string[intoAdd.Length +1];
            if (!intoAdd.Contains(toadd))
            {
                for(int i = 0; i < intoAdd.Length; i++)
                {
                    added[i] = intoAdd[i];
                }
                added[intoAdd.Length] = toadd;
                return added;
            }
            Console.WriteLine(intoAdd.Length);

            return intoAdd;
        }
        public async Task<AccountsDto> UpdateUnfollow(Accounts account, string userId)
        {
            var useraccount = (await _accountTable.FindAsync(x => x._id == userId)).FirstOrDefault();
            var creatoraccount = (await _accountTable.FindAsync(x => x._id == account.followed[0])).FirstOrDefault();
            if (useraccount == null)
            {
                return null;
            }

            var userunfollowed = RemoveFromArray(account.followed[0], useraccount.followed);
            useraccount.followed = userunfollowed;
            var acc = await _accountTable.ReplaceOneAsync(x => x._id == userId, useraccount);

            var creatorunfollowed = RemoveFromArray(account.followed[0], creatoraccount.followers);
            creatoraccount.followers = creatorunfollowed;
            var cacc = await _accountTable.ReplaceOneAsync(x => x._id == account.followed[0], creatoraccount);

            var mapped = useraccount;

            if (acc.IsAcknowledged && cacc.IsAcknowledged)
            {
                return new AccountsDto(mapped._id, mapped.firstname, mapped.lastname, mapped.email, mapped.languages,
                    mapped.followers, mapped.followed, mapped.ppicture, mapped.savedguides, mapped.payedguides);
            }

            return null;
        }
        public async Task<AccountsDto> UpdateAddSaved(Accounts account, string userId)
        {
            var useraccount = (await _accountTable.FindAsync(x => x._id == userId)).FirstOrDefault();
            if (useraccount == null)
            {
                return null;
            }

            var savedGuides = AddToArray(account.savedguides[0], useraccount.savedguides);
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
        public async Task<AccountsDto> UpdateRemoveSaved(Accounts account, string userId)
        {
            var useraccount = (await _accountTable.FindAsync(x => x._id == userId)).FirstOrDefault();
            if (useraccount == null)
            {
                return null;
            }

            var savedGuides = RemoveFromArray(account.savedguides[0], useraccount.savedguides);
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
    }
}
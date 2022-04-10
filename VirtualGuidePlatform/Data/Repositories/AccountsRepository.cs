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
                return new AccountsDto(mapped._id, mapped.email, mapped.languages, mapped.followers, mapped.followed, mapped.ppicture, mapped.savedguides, mapped.payedguides);
            }

            return null;
        }
    }
}
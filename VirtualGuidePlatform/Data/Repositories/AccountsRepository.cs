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
        IMongoQueryable<Accounts> GetAccounts();
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
        public IMongoQueryable<Accounts> GetAccounts()
        {
            var dbList = _accountTable.AsQueryable();
            return dbList;
        }

        public async Task<Accounts> GetAccount(string id)
        {
            var obj = await _accountTable.FindAsync(x => x.Id == ObjectId.Parse(id));
            return obj.FirstOrDefault();
        }
        public async Task<Accounts> CreateAccount(Accounts account)
        {
            var obj = _accountTable.Find(x => x.Id == account.Id).FirstOrDefault();
            if (obj == null)
            {
                await _accountTable.InsertOneAsync(account);
            }
            else
            {
                await _accountTable.ReplaceOneAsync(x => x.Id == account.Id, account);
            }
            return account;
        }
    }
}
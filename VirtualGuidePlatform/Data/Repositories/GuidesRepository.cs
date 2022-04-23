using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualGuidePlatform.Data.Entities;

namespace VirtualGuidePlatform.Data.Repositories
{
    public interface IGuidesRepository
    {
        Task<Guides> CreateGuide(Guides guide);
        Task<Guides> GetGuide(string id);
        Task<List<Guides>> GetGuides();
        Task<List<Guides>> GetUserGuides(string userid);
    }

    public class GuidesRepository : IGuidesRepository
    {
        private MongoClient _mongoClient;
        private IMongoDatabase _database;
        private IMongoCollection<Guides> _guidesTable;
        private IConfiguration _configuration;

        public GuidesRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _mongoClient = new MongoClient(_configuration.GetConnectionString("VVGuideConnection"));
            _database = _mongoClient.GetDatabase("app");
            _guidesTable = _database.GetCollection<Guides>("guides");
        }

        public async Task<Guides> GetGuide(string id)
        {
            return (await _guidesTable.FindAsync(x => x._id == id)).FirstOrDefault();
        }
        public async Task<List<Guides>> GetGuides()
        {
            return await _guidesTable.AsQueryable().ToListAsync();
        }
        public async Task<List<Guides>> GetUserGuides(string userid)
        {
            var res = await _guidesTable.FindAsync(x => x.gCreatorId == userid);
            var items = res.ToList();
            if(items.Count > 0)
            {
                return items;
            }
            return null;
        }
        public async Task<Guides> CreateGuide(Guides guide)
        {
            var obj = (await _guidesTable.FindAsync(x => x._id == guide._id)).FirstOrDefault();

            if (obj == null)
            {
                await _guidesTable.InsertOneAsync(guide);
            }
            else
            {
                await _guidesTable.ReplaceOneAsync(x => x._id == guide._id, guide);
            }

            return guide;
        }
    }
}

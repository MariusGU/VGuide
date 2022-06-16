using Microsoft.AspNetCore.Mvc;
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
        Task<List<Guides>> GetSearchedGuides(Filters filter);
        Task<List<Guides>> GetUserGuides(string userid);
        Task<Guides> SetVisible(string guideId);
        Task<Guides> SetInvisible(string guideId);
        Task<Guides> UpdateGuide(Guides guide);
        Task<bool> DeleteGuide(string guideId);
        Task<List<Guides>> GetCreatorGuides(string userid);
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
            var res = await _guidesTable.FindAsync(x => x.visible == true);
            var items = res.ToList();
            if (items.Count > 0)
            {
                return items;
            }
            return null;
        }
        public async Task<List<Guides>> GetSearchedGuides(Filters filter)
        {
            Console.WriteLine(filter.Category);
            List<Guides> items2;
            if (filter.Category == "" || filter.Category == null)
            {
               var res = await _guidesTable.FindAsync(x => x.name.ToLower().Contains(filter.SearchInput.ToLower()) && x.visible == true);
                items2 = res.ToList();
            }
            else
            {
                var res = await _guidesTable.FindAsync(x => x.category.ToLower() == filter.Category.ToLower() && x.name.ToLower().Contains(filter.SearchInput.ToLower()) && x.visible == true);
                items2 = res.ToList();
            }
            if (items2.Count > 0)
            {
                Console.WriteLine("Ieina cia");
                return items2;
            }
            return null;
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
        public async Task<List<Guides>> GetCreatorGuides(string userid)
        {
            var res = await _guidesTable.FindAsync(x => x.gCreatorId == userid && x.visible);
            var items = res.ToList();
            if (items.Count > 0)
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
        public async Task<Guides> UpdateGuide(Guides guide)
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
        public async Task<Guides> SetVisible(string guideId)
        {
            var obj = (await _guidesTable.FindAsync(x => x._id == guideId)).FirstOrDefault();

            if (obj == null)
            {
                return null;
            }

            Guides updated = obj;
            updated.visible = true;

            var res = await _guidesTable.ReplaceOneAsync(x => x._id == guideId, updated);
            if (res.IsAcknowledged)
            {
                return updated;
            }
            return null;
        }
        public async Task<Guides> SetInvisible(string guideId)
        {
            var obj = (await _guidesTable.FindAsync(x => x._id == guideId)).FirstOrDefault();

            if (obj == null)
            {
                return null;
            }

            Guides updated = obj;
            updated.visible = false;

            var res = await _guidesTable.ReplaceOneAsync(x => x._id == guideId, updated);
            if (res.IsAcknowledged)
            {
                return updated;
            }
            return null;
        }
        public async Task<bool> DeleteGuide(string guideId)
        {
            var res = await _guidesTable.DeleteOneAsync(x => x._id == guideId);

            if (res.IsAcknowledged)
            {
                return true;
            }

            return false;
        }
    }
}

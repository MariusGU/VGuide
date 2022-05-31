using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualGuidePlatform.Data.Entities;

namespace VirtualGuidePlatform.Data.Repositories
{
    public interface IResponsesRepository
    {
        Task<List<Responses>> GetResponses(string guideId);
        Task<Responses> CreateResponse(Responses response);
        Task<Responses> GetUserResponse(string userId, string guideId);
        Task<List<Responses>> GetNotUserResponse(string userid, string guideid);
        Task<bool> DeleteResponse(string rid);
    }

    public class ResponsesRepository : IResponsesRepository
    {
        private MongoClient _mongoClient;
        private IMongoDatabase _database;
        private IMongoCollection<Responses> _responseTable;
        private IConfiguration _configuration;

        public ResponsesRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _mongoClient = new MongoClient(_configuration.GetConnectionString("VVGuideConnection"));
            _database = _mongoClient.GetDatabase("app");
            _responseTable = _database.GetCollection<Responses>("responses");
        }
        public async Task<List<Responses>> GetResponses(string guideId)
        {
            var objects = await _responseTable.FindAsync(x => x.gId == guideId);

            return objects.ToList();
        }
        public async Task<List<Responses>> GetNotUserResponse(string userid, string guideid)
        {
            var items = (await _responseTable.FindAsync(x => x.gId == guideid && x.uId != userid)).ToList();

            return items;
        }
        public async Task<Responses> CreateResponse(Responses response)
        {
            var obj = _responseTable.Find(x => x.gId == response.gId && x.uId == response.uId).FirstOrDefault();
            if (obj == null)
            {
                await _responseTable.InsertOneAsync(response);
            }
            else
            {
                var temp = response;
                temp._id = obj._id;
                await _responseTable.ReplaceOneAsync(x => x._id == temp._id, temp);
            }
            return response;
        }
        public async Task<Responses> GetUserResponse(string userId, string guideId)
        {
            var response = (await _responseTable.FindAsync(x => x.gId == guideId && x.uId == userId)).FirstOrDefault();

            return response;
        }
        public async Task<bool> DeleteResponse(string rid)
        {
            var res = await _responseTable.DeleteOneAsync(x => x._id == rid);
            if (res.IsAcknowledged)
            {
                return true;
            }
            return false;
        }
    }
}

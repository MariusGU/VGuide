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
        public async Task<Responses> CreateResponse(Responses response)
        {
            var obj = _responseTable.Find(x => x._id == response._id).FirstOrDefault();
            if (obj == null)
            {
                await _responseTable.InsertOneAsync(response);
            }
            else
            {
                await _responseTable.ReplaceOneAsync(x => x._id == response._id, response);
            }
            return response;
        }
    }
}

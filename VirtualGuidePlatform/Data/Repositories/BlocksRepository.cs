using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualGuidePlatform.Data.Entities.Blocks;

namespace VirtualGuidePlatform.Data.Repositories
{
    public interface IBlocksRepository
    {
        Task<Pblocks> CreatePblock(Pblocks pblock);
        Task<Tblocks> CreateTblock(Tblocks tblock);
        Task<Vblocks> CreateVblock(Vblocks Vblock);
    }

    public class BlocksRepository : IBlocksRepository
    {
        private MongoClient _mongoClient;
        private IMongoDatabase _database;
        private IMongoCollection<Pblocks> _pBlocksTable;
        private IMongoCollection<Vblocks> _vBlocksTable;
        private IMongoCollection<Tblocks> _tBlocksTable;
        private IConfiguration _configuration;

        public BlocksRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _mongoClient = new MongoClient(_configuration.GetConnectionString("VVGuideConnection"));
            _database = _mongoClient.GetDatabase("app");
            _pBlocksTable = _database.GetCollection<Pblocks>("pblocks");
            _vBlocksTable = _database.GetCollection<Vblocks>("vblocks");
            _tBlocksTable = _database.GetCollection<Tblocks>("tblocks");
        }

        public async Task<Pblocks> CreatePblock(Pblocks pblock)
        {
            var obj = _pBlocksTable.Find(x => x._id == pblock._id).FirstOrDefault();
            if (obj == null)
            {
                await _pBlocksTable.InsertOneAsync(pblock);
            }
            else
            {
                await _pBlocksTable.ReplaceOneAsync(x => x._id == pblock._id, pblock);
            }
            return pblock;
        }
        public async Task<Vblocks> CreateVblock(Vblocks Vblock)
        {
            var obj = _vBlocksTable.Find(x => x._id == Vblock._id).FirstOrDefault();
            if (obj == null)
            {
                await _vBlocksTable.InsertOneAsync(Vblock);
            }
            else
            {
                await _vBlocksTable.ReplaceOneAsync(x => x._id == Vblock._id, Vblock);
            }
            return Vblock;
        }
        public async Task<Tblocks> CreateTblock(Tblocks tblock)
        {
            var obj = _tBlocksTable.Find(x => x._id == tblock._id).FirstOrDefault();
            if (obj == null)
            {
                await _tBlocksTable.InsertOneAsync(tblock);
            }
            else
            {
                await _tBlocksTable.ReplaceOneAsync(x => x._id == tblock._id, tblock);
            }
            return tblock;
        }
    }
}

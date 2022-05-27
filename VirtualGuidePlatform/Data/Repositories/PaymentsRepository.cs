using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Threading.Tasks;
using VirtualGuidePlatform.Data.Entities;

namespace VirtualGuidePlatform.Data.Repositories
{
    public interface IPaymentsRepository
    {
        Task<Payment> CreatePayment(Payment payment);
        Task<Payment> GetPayment(string pid);
    }

    public class PaymentsRepository : IPaymentsRepository
    {
        private MongoClient _mongoClient;
        private IMongoDatabase _database;
        private IMongoCollection<Payment> _paymentsTable;
        private IConfiguration _configuration;

        public PaymentsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _mongoClient = new MongoClient(_configuration.GetConnectionString("VVGuideConnection"));
            _database = _mongoClient.GetDatabase("app");
            _paymentsTable = _database.GetCollection<Payment>("payments");
        }

        public async Task<Payment> CreatePayment(Payment payment)
        {
            var obj = _paymentsTable.Find(x => x._id == payment._id).FirstOrDefault();
            if (obj == null)
            {
                await _paymentsTable.InsertOneAsync(payment);
            }
            else
            {
                await _paymentsTable.ReplaceOneAsync(x => x._id == payment._id, payment);
            }
            return payment;
        }
        public async Task<Payment> GetPayment(string pid)
        {
            var obj = (await _paymentsTable.FindAsync(x => x.pID == pid)).FirstOrDefault();

            if (obj != null)
            {
                return obj;
            }

            return null;
        }
    }
}

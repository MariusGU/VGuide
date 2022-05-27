using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VirtualGuidePlatform.Data.Entities
{
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string uID { get; set; }
        public string gID { get; set; }
        public string pID { get; set; }
    }
}

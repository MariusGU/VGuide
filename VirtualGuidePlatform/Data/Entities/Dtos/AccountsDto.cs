using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VirtualGuidePlatform.Data.Entities
{
    public class AccountsDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        [BsonElement("email")]
        public string email { get; set; }
        [BsonElement("languages")]
        public string[] languages { get; set; }
        [BsonElement("followers")]
        public string[] followers { get; set; }
        [BsonElement("followed")]
        public string[] followed { get; set; }
        [BsonElement("ppicture")]
        public string ppicture { get; set; }
        [BsonElement("savedguides")]
        public string[] savedguides { get; set; }
        [BsonElement("payedguides")]
        public string[] payedguides { get; set; }
    }
}

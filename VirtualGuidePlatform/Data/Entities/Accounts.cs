using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VirtualGuidePlatform.Data.Entities
{
    [BsonIgnoreExtraElements]
    public class Accounts
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        [BsonElement("firstname")]
        public string firstname { get; set; }
        [BsonElement("lastname")]
        public string lastname { get; set; }
        [BsonElement("email")]
        public string email { get; set; }
        [BsonElement("password")]
        public string password { get; set; }
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
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VirtualGuidePlatform.Data.Entities
{
    public class Responses
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public string text { get; set; }
        public double rating { get; set; }
        public string gId { get; set; }
        public string uId { get; set; }

    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VirtualGuidePlatform.Data.Entities
{
    public class Guides
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public int gCreatorId { get; set; }
        public string locationXY { get; set; }
        public string description { get; set; }
        public string city { get; set; }
        public string name { get; set; }
        public string language { get; set; }
        public DateTime uDate { get; set; }
        public double price { get; set; }
    }
}

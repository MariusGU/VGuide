using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VirtualGuidePlatform.Data.Entities.Blocks
{
    public class Vblocks
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public int priority { get; set; }
        public string URI { get; set; }
        public string FileName { get; set; }
        public string gId { get; set; }
    }
}

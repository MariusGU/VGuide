﻿using MongoDB.Bson;
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
        public string gCreatorId { get; set; }
        public double latitude { get; set; }
        public double longtitude { get; set; }
        public string description { get; set; }
        public string city { get; set; }
        public string name { get; set; }
        public string language { get; set; }
        public DateTime uDate { get; set; }
        public double price { get; set; }
        public bool visible { get; set; }
        public string category { get; set; }
    }
}

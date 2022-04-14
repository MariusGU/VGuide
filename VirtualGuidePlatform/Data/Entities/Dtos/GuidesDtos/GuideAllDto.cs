using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VirtualGuidePlatform.Data.Entities.Dtos
{
    public class GuideAllDto
    {
        public string Image { get; set; }
        public string _id { get; set; }
        public string creatorName { get; set; }
        public string creatorLastName { get; set; }
        public string creatorId { get; set; }
        public string description { get; set; }
        public double latitude { get; set; }
        public double longtitude { get; set; }
        public string city { get; set; }
        public string title { get; set; }
        public string language { get; set; }
        public DateTime uDate { get; set; }
        public double price { get; set; }
        public double rating { get; set; }
        public bool isFavourite { get; set; }
        public bool visible { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualGuidePlatform.Data.Entities.Blocks;

namespace VirtualGuidePlatform.Data.Entities.Dtos
{
    public class BlockDto
    {
        public string Type { get; set; }
        public int ID { get; set; }
        public FileContentResult? image { get; set; }
        public Pblocks? pblock { get; set; }
        public FileContentResult? video { get; set; }
        public Vblocks? vblock { get; set; }
        public Tblocks? tblock { get; set; }
    }
}

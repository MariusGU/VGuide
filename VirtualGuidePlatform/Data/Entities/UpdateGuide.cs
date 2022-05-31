using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace VirtualGuidePlatform.Data.Entities
{
    public class UpdateGuide
    {
        public IFormFile[] Images { get; set; }
        public IFormFile[] Videos { get; set; }
        public string[] VideosUris { get; set; }
        public string[] ImagesUris { get; set; }
        public string[] Texts { get; set; }
        public string[] Blocks { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public double Price { get; set; }
        public string CreatorId { get; set; }
        public string latitude { get; set; }
        public string longtitude { get; set; }
        public string City { get; set; }
        public bool Visible { get; set; }
        public string Category { get; set; }
        public string GuideId { get; set; }

        public List<Block>? DeserializedBlocks { get; set; }
        public void DeserializeBlocks()
        {
            this.DeserializedBlocks = new List<Block>();
            foreach (var block in this.Blocks)
            {
                Block deserialized = JsonSerializer.Deserialize<Block>(block);
                this.DeserializedBlocks.Add(deserialized);
            }
        }
    }
}

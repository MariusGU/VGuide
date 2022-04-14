using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text.Json;

namespace VirtualGuidePlatform.Data.Entities
{
    public class Block
    {
        public int ID { get; set; }
        public string Type { get; set; }
    }

    public class PostGuide
    {
        public IFormFile[] Images { get; set; }
        public IFormFile[] Videos { get; set; }
        public string[] Texts { get; set; }
        public string[] Blocks { get; set; }

        public string CreatorId { get; set; }
        public double latitude { get; set; }
        public double longtitude { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Title { get; set; }
        public string Language { get; set; }
        public double Price { get; set; }
        public bool Visible { get; set; }

        public List<Block>? DeserializedBlocks { get; set; }
        public void DeserializeBlocks()
        {
            this.DeserializedBlocks = new List<Block>();
            foreach(var block in this.Blocks)
            {
                Block deserialized = JsonSerializer.Deserialize<Block>(block);
                this.DeserializedBlocks.Add(deserialized);
            }
        }
    }
    
}

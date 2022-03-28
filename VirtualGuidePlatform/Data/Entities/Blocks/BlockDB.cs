namespace VirtualGuidePlatform.Data.Entities.Blocks
{
    public class BlockDB
    {
        public string Type { get; set; }
        public VideoBlock? vidBlock { get; set; }
        public ImageBlock? imgBlock { get; set; }
        public TextBlock? txtBlock { get; set; }

    }
}

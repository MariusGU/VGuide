namespace VirtualGuidePlatform.Data.Entities.Blocks
{
    public class BlockDB
    {
        public string Type { get; set; }
        public Vblocks? vidBlock { get; set; }
        public Pblocks? imgBlock { get; set; }
        public Tblocks? txtBlock { get; set; }

    }
}

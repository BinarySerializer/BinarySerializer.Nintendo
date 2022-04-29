namespace BinarySerializer.Nintendo.GBA
{
    public static class Constants
    {
        // Addresses
        public const uint Address_WRAM = 0x2000000;  // Size 0x40000
        public const uint Address_IRAM = 0x3000000;  // Size 0x08000
        public const uint Address_IO = 0x4000000;  // Size 0x003FF
        public const uint Address_PAL = 0x5000000;  // Size 0x04000
        public const uint Address_VRAM = 0x6000000;  // Size 0x18000
        public const uint Address_OAM = 0x7000000;  // Size 0x00400
        public const uint Address_ROM = 0x08000000; // Size 0x1000000

        // GBA values
        public const int TileSize = 8;
        public const int ScreenWidth = 240;
        public const int ScreenHeight = 160;
        public static Size[] SpriteShapes { get; } = 
        {
            new Size(0x08, 0x08),
            new Size(0x10, 0x10),
            new Size(0x20, 0x20),
            new Size(0x40, 0x40),
            new Size(0x10, 0x08),
            new Size(0x20, 0x08),
            new Size(0x20, 0x10),
            new Size(0x40, 0x20),
            new Size(0x08, 0x10),
            new Size(0x08, 0x20),
            new Size(0x10, 0x20),
            new Size(0x20, 0x40),
        };

        public static Size GetSpriteShape(int shape, int size) => SpriteShapes[shape * 4 + size];
        public static Size GetSpriteShape(this OBJ_ATTR attr) => SpriteShapes[attr.SpriteShape * 4 + attr.SpriteSize];

        public class Size
        {
            public Size(int width, int height)
            {
                Width = width;
                Height = height;

                TilesWidth = width / TileSize;
                TilesHeight = height / TileSize;

                TilesCount = TilesWidth * TilesHeight;
            }

            public int Width { get; }
            public int Height { get; }

            public int TilesWidth { get; }
            public int TilesHeight { get; }

            public int TilesCount { get; }
        }
    }
}
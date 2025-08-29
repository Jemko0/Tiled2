namespace Tiled.World
{
    public struct FPackedLight
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;

        public FPackedLight(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public static uint PackRGBA(byte r, byte g, byte b, byte a = 255)
        {
            return ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | b;
        }

        public static FPackedLight UnpackRGBA(ref uint packed)
        {
            byte a = (byte)((packed >> 24) & 0xFF);
            byte r = (byte)((packed >> 16) & 0xFF);
            byte g = (byte)((packed >> 8) & 0xFF);
            byte b = (byte)(packed & 0xFF);
            return new FPackedLight(r, g, b, a);
        }
    }
}

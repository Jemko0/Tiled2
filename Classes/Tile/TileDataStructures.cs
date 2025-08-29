using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tiled
{
    public enum ETileType
    {
        AIR = 0,
        GRASS,
        DIRT,
        STONE,
        
        MAX
    };

    public struct int2
    {
        public int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static int2 copy(ref int2 other)
        {
            return new int2(other.x, other.y);
        }

        public int x;
        public int y;
    }

    public struct int4
    {
        public int4(int x, int y, int z, int w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static int4 copy(ref int4 other)
        {
            return new int4(other.x, other.y, other.z, other.w);
        }

        public int x;
        public int y;
        public int z;
        public int w;
    }
    public struct FTile
    {
        public ETileType type;
        public bool solid;
        public int2 frame;
    }

    public class TileData
    {
        public FTile GetTile(ETileType type)
        {
            FTile tile = new FTile();
            tile.type = type;
            tile.solid = true;

            switch (type)
            {
                default:
                    break;

                case ETileType.AIR:
                    tile.solid = false;
                    tile.frame = new int2(0, 0);
                    break;
            }

            return tile;
        }
    }
}

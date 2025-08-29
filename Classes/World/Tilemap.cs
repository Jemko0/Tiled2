using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.Framework;

namespace Tiled.World
{
    public class Tilemap
    {
        public Tilemap() { }

        public ETileType[,] tiles;
        public uint[,] lightmap;

        public void Initialize(uint width, uint height)
        {
            tiles = new ETileType[width, height];
            lightmap = new uint[width, height];

            for (int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    tiles[x, y] = (y > ((height / 2) + Random.Shared.Next(100) - 50))? (ETileType)Random.Shared.Next((int)ETileType.MAX): ETileType.AIR;
                }
            }
        }
    }
}

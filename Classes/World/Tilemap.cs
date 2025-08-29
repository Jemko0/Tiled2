using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled.Framework;
using Tiled2.Rendering;

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
                    tiles[x, y] = (y > height / 2)? (ETileType)Random.Shared.Next((int)ETileType.MAX): ETileType.AIR;
                }
            }
        }

        public static int4 GetTileIndicesInActiveCamera()
        {
            if(!GameState.Instance.initialized)
            {
                return new int4(0, 0, 0, 0);
            }

            Vector2 camPos = GameState.Instance.GetActiveCamera().position;
            float camZoom = GameState.Instance.GetActiveCamera().zoom;

            int tilesWidth = (int)(Renderer.state.ViewportBounds.Width / (16 * camZoom));
            int tilesHeight = (int)(Renderer.state.ViewportBounds.Height / (16 * camZoom));

            //top left most tile
            int tileX = (int)(camPos.X / 16) - tilesWidth / 2;
            int tileY = (int)(camPos.Y / 16) - tilesHeight / 2;

            return new int4(tileX, tileY, tilesWidth, tilesHeight);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Tiled;
using Tiled.Framework;
using Tiled.World;

namespace Tiled2.Rendering
{
    public struct RendererState
    {
        public Rectangle ViewportBounds;
    }

    public class Renderer
    {
        public Renderer() { }

        public static GraphicsDevice graphicsDevice;
        public static SpriteBatch spriteBatch;
        public static GraphicsDeviceManager graphicsDeviceManager;
        public static ContentManager content;
        public static RendererState state;

        //shaders
        public Effect baseTileShader;

        Texture2D pixelTexture;

        public void Init(ref SpriteBatch s, ref GraphicsDeviceManager gdm, ContentManager contentMgr)
        {
            state = new RendererState();

            spriteBatch = s;
            graphicsDeviceManager = gdm;
            graphicsDevice = gdm.GraphicsDevice;
            content = contentMgr;

            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });

            baseTileShader = content.Load<Effect>("Shaders/BaseTileShader");
            //baseLightingShader = content.Load<Effect>("Shaders/BaseLightingShader");
        }

        public void CalculateViewport(Game g)
        {
            state.ViewportBounds = g.Window.ClientBounds;
        }

        public void Render()
        {
            RenderTiles();
            RenderLighting();
        }

        public static Matrix GetWorldViewProjection()
        {
            float camX = GameState.Instance.GetActiveCamera().position.X;
            float camY = GameState.Instance.GetActiveCamera().position.Y;
            float camZoom = GameState.Instance.GetActiveCamera().zoom;

            Matrix world = Matrix.Identity;
            Matrix view = Matrix.CreateTranslation(-camX, -camY, 0) * Matrix.CreateScale(camZoom);

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width,
                                                                   graphicsDevice.Viewport.Height, 0, 0, 1);

            Matrix worldViewProjection = world * view * projection;

            return worldViewProjection;
        }

        #region TILE RENDERING

        public struct InstanceData : IVertexType
        {
            public Vector2 Position;
            public Vector4 Data; // tile type, color, etc.

            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(8, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2)
            );

            VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
        }

        private VertexBuffer baseVertexBuffer;
        private VertexBuffer instanceBuffer;
        private IndexBuffer indexBuffer;
        private InstanceData[] tileInstances;
        private int actualTileCount;
        private const int TILE_SIZE = 16;

        public void InitializeTileBuffers()
        {
            VertexPositionTexture[] baseVertices = new VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 0)),           // Top-left
                new VertexPositionTexture(new Vector3(TILE_SIZE, 0, 0), new Vector2(1, 0)),   // Top-right
                new VertexPositionTexture(new Vector3(0, TILE_SIZE, 0), new Vector2(0, 1)),   // Bottom-left
                new VertexPositionTexture(new Vector3(TILE_SIZE, TILE_SIZE, 0), new Vector2(1, 1)), // Bottom-right
            };

            baseVertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
            baseVertexBuffer.SetData(baseVertices);

            // Create index buffer for two triangles (quad)
            short[] indices = new short[]
            {
                0, 1, 2,  // First triangle
                2, 1, 3   // Second triangle
            };

            indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);

            var tilemap = GameState.Instance.currentTilemap.tiles;
            int4 bounds = Tilemap.GetTileIndicesInActiveCamera();
            tileInstances = new InstanceData[bounds.z * bounds.w];

            if(bounds.z <= 0 || bounds.w <= 0)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine("FRUSTUM TILES: " + bounds.x, bounds.y, bounds.z, bounds.w);

            int index = 0;
            for (int x = bounds.x; x < bounds.x + bounds.z; x++)
            {
                for (int y = bounds.y; y < bounds.y + bounds.w; y++)
                {
                    if(!Utility.UtilityLibrary.IsValidIndex(tilemap, [x, y]))
                    {
                        continue;
                    }

                    ETileType tileType = tilemap[x, y];

                    tileInstances[index] = new InstanceData
                    {
                        Position = new Vector2(x * TILE_SIZE, y * TILE_SIZE),
                        Data = new Vector4((float)tileType, x, y, 0)
                    };
                    index++;
                }
            }

            instanceBuffer = new VertexBuffer(graphicsDevice, typeof(InstanceData), tileInstances.Length, BufferUsage.WriteOnly);
            instanceBuffer.SetData(tileInstances);
        }

        public void RenderTiles()
        {      
            InitializeTileBuffers();
            if(instanceBuffer == null)
            {
                return;
            }

            graphicsDevice.SetVertexBuffers(
                new VertexBufferBinding(baseVertexBuffer, 0, 0),
                new VertexBufferBinding(instanceBuffer, 0, 1)
            );

            graphicsDevice.Indices = indexBuffer;

            // Set the WorldViewProjection matrix parameter
            baseTileShader.Parameters["WorldViewProjection"]?.SetValue(GetWorldViewProjection());

            baseTileShader.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2, tileInstances.Length);
        }

        public void RenderLighting()
        {

        }

        #endregion
    }
}
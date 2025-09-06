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
using Tiled2.Framework.Gameplay;

namespace Tiled2.Rendering
{
    public class Renderer
    {
        public Renderer() { }

        public static GraphicsDevice graphicsDevice;
        public static SpriteBatch spriteBatch;
        public static GraphicsDeviceManager graphicsDeviceManager;
        public static ContentManager content;

        //shaders
        public Effect baseTileShader;
        public Effect entityShader;

        public RenderTarget2D lightingRT;

        Texture2D pixelTexture;

        // Optimized tile rendering members
        private VertexBuffer baseVertexBuffer;
        private DynamicVertexBuffer instanceBuffer; // Changed to dynamic for frequent updates
        private IndexBuffer indexBuffer;
        private InstanceData[] visibleTileInstances;
        private int maxVisibleTiles;
        private const int TILE_SIZE = 16;

        // View culling parameters
        private Rectangle lastViewBounds = Rectangle.Empty;
        private bool needsBufferUpdate = true;

        public void Init(ref SpriteBatch s, ref GraphicsDeviceManager gdm, ContentManager contentMgr)
        {
            spriteBatch = s;
            graphicsDeviceManager = gdm;
            graphicsDevice = gdm.GraphicsDevice;
            content = contentMgr;

            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });

            baseTileShader = content.Load<Effect>("Shaders/BaseTileShader");
            entityShader = content.Load<Effect>("Shaders/EntityShader");
            lightingRT = new RenderTarget2D(graphicsDevice, 1024, 1024);

            InitializeTileBuffers();
        }

        public void Render()
        {
            UpdateVisibleTiles();
            RenderTiles();
            RenderEntities();
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

        public void InitializeTileBuffers()
        {
            // Create base quad vertices
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

            // Estimate maximum visible tiles based on screen size and minimum zoom
            float minZoom = 0.1f; // Adjust based on your game's minimum zoom level
            int screenTilesX = (int)Math.Ceiling(graphicsDevice.Viewport.Width / (TILE_SIZE * minZoom)) + 2; // +2 for safety margin
            int screenTilesY = (int)Math.Ceiling(graphicsDevice.Viewport.Height / (TILE_SIZE * minZoom)) + 2;
            maxVisibleTiles = screenTilesX * screenTilesY;

            // Create dynamic instance buffer for visible tiles only
            visibleTileInstances = new InstanceData[maxVisibleTiles];
            instanceBuffer = new DynamicVertexBuffer(graphicsDevice, typeof(InstanceData), maxVisibleTiles, BufferUsage.WriteOnly);
        }

        private Rectangle GetViewBounds()
        {
            var camera = GameState.Instance.GetActiveCamera();
            float camX = camera.position.X;
            float camY = camera.position.Y;
            float camZoom = camera.zoom;

            // Calculate world space bounds of what's visible on screen
            int viewWidth = (int)(graphicsDevice.Viewport.Width / camZoom);
            int viewHeight = (int)(graphicsDevice.Viewport.Height / camZoom);

            // Add margin for tiles that are partially visible
            int margin = TILE_SIZE * 2;

            return new Rectangle(
                (int)(camX - margin),
                (int)(camY - margin),
                viewWidth + margin * 2,
                viewHeight + margin * 2
            );
        }

        private void UpdateVisibleTiles()
        {
            Rectangle currentViewBounds = GetViewBounds();

            // Only update if the view has changed significantly
            if (currentViewBounds != lastViewBounds)
            {
                lastViewBounds = currentViewBounds;
                needsBufferUpdate = true;
            }

            if (!needsBufferUpdate) return;

            var tilemap = GameState.Instance.currentTilemap.tiles;
            int tilemapWidth = tilemap.GetLength(0);
            int tilemapHeight = tilemap.GetLength(1);

            // Convert view bounds to tile coordinates
            int startX = Math.Max(0, currentViewBounds.Left / TILE_SIZE);
            int endX = Math.Min(tilemapWidth - 1, currentViewBounds.Right / TILE_SIZE);
            int startY = Math.Max(0, currentViewBounds.Top / TILE_SIZE);
            int endY = Math.Min(tilemapHeight - 1, currentViewBounds.Bottom / TILE_SIZE);

            int visibleTileCount = 0;

            // Only loop through visible tiles
            for (int x = startX; x <= endX && visibleTileCount < maxVisibleTiles; x++)
            {
                for (int y = startY; y <= endY && visibleTileCount < maxVisibleTiles; y++)
                {
                    ETileType tileType = tilemap[x, y];

                    // Skip empty/air tiles if desired
                    if (tileType == ETileType.AIR) continue;

                    visibleTileInstances[visibleTileCount] = new InstanceData
                    {
                        Position = new Vector2(x * TILE_SIZE, y * TILE_SIZE),
                        Data = new Vector4((float)tileType, x, y, 0)
                    };
                    visibleTileCount++;
                }
            }

            // Update the instance buffer with only visible tiles
            if (visibleTileCount > 0)
            {
                instanceBuffer.SetData(visibleTileInstances, 0, visibleTileCount, SetDataOptions.Discard);
            }

            actualTileCount = visibleTileCount;
            needsBufferUpdate = false;
        }

        private int actualTileCount = 0;

        public void RenderTiles()
        {
            if (actualTileCount == 0) return; // Nothing to render

            graphicsDevice.SetVertexBuffers(
                new VertexBufferBinding(baseVertexBuffer, 0, 0),
                new VertexBufferBinding(instanceBuffer, 0, 1)
            );

            graphicsDevice.Indices = indexBuffer;

            // Set the WorldViewProjection matrix parameter
            baseTileShader.Parameters["WorldViewProjection"]?.SetValue(GetWorldViewProjection());

            baseTileShader.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2, actualTileCount);
        }

        #endregion

        public void RenderEntities()
        {
            entityShader.Parameters["WorldViewProjection"]?.SetValue(GetWorldViewProjection());
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, entityShader);

            // Also cull entities that are outside the view
            Rectangle viewBounds = GetViewBounds();

            for (int i = 0; i < GameState.Instance.entities.Count; i++)
            {
                Entity e = GameState.Instance.entities[i];
                Rectangle entityBounds = new Rectangle(
                    (int)e.transform.position.X,
                    (int)e.transform.position.Y,
                    (int)e.transform.scale.X,
                    (int)e.transform.scale.Y
                );

                // Only render entities that intersect with the view
                if (viewBounds.Intersects(entityBounds))
                {
                    spriteBatch.Draw(pixelTexture, entityBounds, Color.White);
                }
            }

            spriteBatch.End();
        }

        // Call this when the tilemap changes to force an update
        public void ForceBufferUpdate()
        {
            needsBufferUpdate = true;
        }
    }
}
using Microsoft.Xna.Framework;
using System;
using Tiled;
using Tiled.World;

namespace Tiled2.Framework.Gameplay.Components
{
    public class CollisionComponent : Component
    {
        public CollisionComponent(object owner) : base(owner) { }

        /// <summary>
        /// Performs optimized collision detection against tilemap
        /// Only checks tiles in the immediate area around the entity
        /// </summary>
        /// <param name="position">Current position</param>
        /// <param name="velocity">Current velocity</param>
        /// <param name="deltaTime">Frame delta time</param>
        /// <param name="tilemap">The tilemap to check against</param>
        /// <param name="entitySize">Size of the entity (width, height)</param>
        /// <returns>New position after collision resolution</returns>
        public Vector2 CheckCollision(Vector2 position, Vector2 velocity, float deltaTime, Tilemap tilemap, Vector2 entitySize)
        {
            if (tilemap?.tiles == null || velocity == Vector2.Zero)
                return position;

            Vector2 newPosition = position + velocity * deltaTime;

            // Check X-axis collision first
            Vector2 positionAfterX = new Vector2(newPosition.X, position.Y);
            positionAfterX = ResolveAxisCollision(positionAfterX, position, tilemap, entitySize, true);

            // Then check Y-axis collision
            Vector2 finalPosition = new Vector2(positionAfterX.X, newPosition.Y);
            finalPosition = ResolveAxisCollision(finalPosition, positionAfterX, tilemap, entitySize, false);

            return finalPosition;
        }

        /// <summary>
        /// Resolves collision for a specific axis (X or Y)
        /// </summary>
        private Vector2 ResolveAxisCollision(Vector2 newPosition, Vector2 oldPosition, Tilemap tilemap, Vector2 entitySize, bool isXAxis)
        {
            // Calculate entity bounds
            Rectangle entityBounds = new Rectangle(
                (int)newPosition.X,
                (int)newPosition.Y,
                (int)entitySize.X,
                (int)entitySize.Y
            );

            // Calculate tile range to check (only tiles that could intersect with entity)
            int minTileX = Math.Max(0, (entityBounds.Left - 1) / Tilemap.TILESIZE);
            int maxTileX = Math.Min(tilemap.tiles.GetLength(0) - 1, (entityBounds.Right + 1) / Tilemap.TILESIZE);
            int minTileY = Math.Max(0, (entityBounds.Top - 1) / Tilemap.TILESIZE);
            int maxTileY = Math.Min(tilemap.tiles.GetLength(1) - 1, (entityBounds.Bottom + 1) / Tilemap.TILESIZE);

            // Check only the relevant tiles
            for (int x = minTileX; x <= maxTileX; x++)
            {
                for (int y = minTileY; y <= maxTileY; y++)
                {
                    if (IsTileSolid(tilemap.tiles[x, y]))
                    {
                        Rectangle tileBounds = new Rectangle(x * Tilemap.TILESIZE, y * Tilemap.TILESIZE, Tilemap.TILESIZE, Tilemap.TILESIZE);

                        if (entityBounds.Intersects(tileBounds))
                        {
                            // Collision detected - resolve it
                            if (isXAxis)
                            {
                                // X-axis collision resolution
                                if (newPosition.X > oldPosition.X) // Moving right
                                {
                                    newPosition.X = tileBounds.Left - entitySize.X;
                                }
                                else // Moving left
                                {
                                    newPosition.X = tileBounds.Right;
                                }
                            }
                            else
                            {
                                // Y-axis collision resolution
                                if (newPosition.Y > oldPosition.Y) // Moving down
                                {
                                    newPosition.Y = tileBounds.Top - entitySize.Y;
                                }
                                else // Moving up
                                {
                                    newPosition.Y = tileBounds.Bottom;
                                }
                            }

                            // Update bounds after resolution for subsequent checks
                            entityBounds = new Rectangle(
                                (int)newPosition.X,
                                (int)newPosition.Y,
                                (int)entitySize.X,
                                (int)entitySize.Y
                            );
                        }
                    }
                }
            }

            return newPosition;
        }

        /// <summary>
        /// Check if a specific tile type is solid/collidable
        /// </summary>
        private bool IsTileSolid(ETileType tileType)
        {
            // Modify this based on your ETileType enum
            // Assuming AIR is non-solid, everything else is solid
            return tileType != ETileType.AIR;
        }

        /// <summary>
        /// Quick check if a point is colliding with solid tiles
        /// Useful for simple point-based collision queries
        /// </summary>
        public bool IsPointColliding(Vector2 point, Tilemap tilemap)
        {
            if (tilemap?.tiles == null)
                return false;

            int tileX = (int)(point.X / Tilemap.TILESIZE);
            int tileY = (int)(point.Y / Tilemap.TILESIZE);

            // Bounds check
            if (tileX < 0 || tileX >= tilemap.tiles.GetLength(0) ||
                tileY < 0 || tileY >= tilemap.tiles.GetLength(1))
                return true; // Consider out-of-bounds as collision

            return IsTileSolid(tilemap.tiles[tileX, tileY]);
        }

        /// <summary>
        /// Check if an entity can move to a specific position
        /// </summary>
        public bool CanMoveTo(Vector2 position, Tilemap tilemap, Vector2 entitySize)
        {
            Rectangle entityBounds = new Rectangle(
                (int)position.X,
                (int)position.Y,
                (int)entitySize.X,
                (int)entitySize.Y
            );

            int minTileX = Math.Max(0, entityBounds.Left / Tilemap.TILESIZE);
            int maxTileX = Math.Min(tilemap.tiles.GetLength(0) - 1, entityBounds.Right / Tilemap.TILESIZE);
            int minTileY = Math.Max(0, entityBounds.Top / Tilemap.TILESIZE);
            int maxTileY = Math.Min(tilemap.tiles.GetLength(1) - 1, entityBounds.Bottom / Tilemap.TILESIZE);

            for (int x = minTileX; x <= maxTileX; x++)
            {
                for (int y = minTileY; y <= maxTileY; y++)
                {
                    if (IsTileSolid(tilemap.tiles[x, y]))
                    {
                        Rectangle tileBounds = new Rectangle(x * Tilemap.TILESIZE, y * Tilemap.TILESIZE, Tilemap.TILESIZE, Tilemap.TILESIZE);
                        if (entityBounds.Intersects(tileBounds))
                            return false;
                    }
                }
            }

            return true;
        }
    }
}
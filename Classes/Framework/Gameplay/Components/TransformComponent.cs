using Microsoft.Xna.Framework;
using System.Text.Json.Nodes;

namespace Tiled2.Framework.Gameplay.Components
{
    public class TransformComponent : Component
    {
        public Vector2 position;
        public float rotation;
        public Vector2 scale;

        public TransformComponent(Entity owner) : base(owner) { }

        public void SetPosition(Vector2 newPosition)
        {
            position = newPosition;
        }

        public void SetRotation(float newRotation)
        {
            rotation = newRotation;
        }

        public void SetScale(Vector2 newScale)
        {
            scale = newScale;
        }
    }
}

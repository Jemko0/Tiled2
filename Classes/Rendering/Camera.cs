using Microsoft.Xna.Framework;

namespace Tiled.Rendering
{
    public class Camera
    {
        public Vector2 position;
        public Vector2 rotation;
        public float zoom = 1.0f;

        public void SetPosition(Vector2 position)
        {
            this.position = position;
        }
    }
}

using System.Runtime.CompilerServices;
using Tiled.Rendering;
using Tiled.World;

namespace Tiled.Framework
{
    public class GameState
    {
        private static GameState _instance;

        private static Camera activeCamera;
        public Tilemap currentTilemap;
        public bool initialized = false;

        private GameState() { }

        public static GameState Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameState();
                }
                    
                return _instance;
            }
        }

        public void SetActiveCamera(ref Camera cam)
        {
            activeCamera = cam;
        }

        public  ref Camera GetActiveCamera()
        {
            return ref activeCamera;
        }

        public void Initialize()
        {
            CreateCamera();
            CreateTileMap();
            initialized = true;
        }

        public void CreateCamera()
        {
            if(activeCamera != null)
            {
                return;
            }

            activeCamera = new Camera();
            activeCamera.SetPosition(new Microsoft.Xna.Framework.Vector2(0, 0));
        }

        public void CreateTileMap()
        {
            currentTilemap = new Tilemap();
            currentTilemap.Initialize(8400, 2400);
        }
    }
}

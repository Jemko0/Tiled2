using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiled2.Framework.Gameplay.Components;

namespace Tiled2.Framework.Gameplay.Entities
{
    public class MovingEntity : Entity
    {
        public MovingEntity() { }
        public CollisionComponent collision;
        public MovementComponent movement;

        public override void Initialize()
        {
            base.Initialize();

            collision = new CollisionComponent(this);
            movement = new MovementComponent(this);

            RegisterComponents([collision, movement]);
        }
    }
}

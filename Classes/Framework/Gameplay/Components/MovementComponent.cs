using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2.Framework.Gameplay.Components
{
    public class MovementComponent : Component
    {
        public MovementComponent(object owner) : base(owner) { }

        float maxSpeed = 500.0f;
        float acceleration = 1000.0f;
        Vector2 velocity = Vector2.Zero;
        Vector2 lastInputVector = Vector2.Zero;

        public void SendInputVector(Vector2 input)
        {
            lastInputVector = input;
        }

        public Vector2 ConsumeInputVector()
        {
            Vector2 v = lastInputVector;
            lastInputVector = Vector2.Zero;

            return v;
        }

        public override void UpdateComponent()
        {
            base.UpdateComponent();
            Vector2 input = ConsumeInputVector();

            velocity.X = Math.Clamp(velocity.X + (acceleration *= input.X), -maxSpeed, maxSpeed);
        }
    }
}

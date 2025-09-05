using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Tiled2.Framework.Gameplay.Components
{
    public class Component : IDisposable
    {
        object Owner;

        public Component(object owner)
        {
            Owner = owner;
        }

        public virtual void InitializeComponent(JsonObject?[]? args)
        {
        }

        public object? GetOwner()
        {
            return Owner;
        }

        public virtual void UpdateComponent()
        {

        }

        public void Dispose()
        {
            return;
        }
    }
}

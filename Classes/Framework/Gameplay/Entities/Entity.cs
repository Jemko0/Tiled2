using System;
using System.Collections.Generic;
using Tiled2.Framework.Gameplay.Components;
using Tiled2.Framework.Gameplay.Interface;

namespace Tiled2.Framework.Gameplay
{
    public class Entity : IObjectQuery, IDisposable
    {
        public Entity() { }

        public List<Component> components;
        public TransformComponent transform;

        public virtual void Initialize()
        {
            components = new List<Component>();
        }

        public void Update()
        {
            for(int i = 0; i < components.Count; i++)
            {
                components[i].UpdateComponent();
            }
        }

        public void RegisterComponents(Component?[]? components)
        {
            for (int i = 0; i < components.Length; i++)
            {
                this.components.Add(components[i]);
            }
        }

        public T[] GetComponent<T>() where T : Component
        {
            T[] values = new T[components.Count];

            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].GetType() == typeof(T))
                {
                    values[i] = (T)components[i];
                }
            }

            return values;
        }

        public void Dispose()
        {
            for(int i = 0; i < components.Count; i++)
            {
                components[i].Dispose();
            }

            components.Clear();
            GC.Collect();
        }
    }
}

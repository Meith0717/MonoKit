// Physics.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;

namespace Engine.Gameplay
{
    public class Physics
    {
        private static void MomentumConservation(float m1, float m2, float v1, float v2, out float v)
        {
            v = 0;
            if (float.IsInfinity(m1) || float.IsInfinity(m2)) return;
            if (v1 == 0 && v2 == 0) return;
            v = (m1 * v1 + m2 * v2) / (m1 + m2);
        }


        private static bool TryGetGetMass(GameObject gameObject2D, out float mass)
        {
            mass = 0;
            if (gameObject2D is not ICollidable) return false;
            ICollidable collidable = (ICollidable)gameObject2D;
            mass = collidable.Mass;
            return true;
        }

        [Obsolete]
        public static void HandleCollision(GameTime gameTime, GameObject gameObject2D, SpatialHashing spatialHashing)
        {
            if (!TryGetGetMass(gameObject2D, out float m1)) return;
            System.Collections.Generic.List<GameObject> objts = spatialHashing.GetObjectsInRadius<GameObject>(gameObject2D.Position, gameObject2D.BoundBox.Radius);

            Vector2 acceleration = Vector2.Zero;

            foreach (GameObject obj in objts)
            {
                if (obj == gameObject2D) continue;
                if (!TryGetGetMass(obj, out float m2)) continue;
                if (!CollisionDetection.Collide(gameTime, gameObject2D, obj, out Vector2? _)) continue;

                // Calc direction
                Vector2 pushDir = Vector2.Normalize(gameObject2D.Position - obj.Position);
                if (pushDir.IsNaN())
                    pushDir = Vector2.UnitX;

                MomentumConservation(m1, m2, gameObject2D.Velocity, obj.Velocity, out float v);
                acceleration += pushDir * v;
            }
            if (acceleration.X == 0 & acceleration.Y == 0) return;
            gameObject2D.MovingDirection += acceleration.NormalizedCopy();
            gameObject2D.Velocity *= acceleration.Length();
        }

        public static void HandleCollision(double elapsedMs, GameObject gameObject2D, SpatialHashing spatialHashing)
        {
            if (!TryGetGetMass(gameObject2D, out float m1)) return;
            System.Collections.Generic.List<GameObject> objts = spatialHashing.GetObjectsInRadius<GameObject>(gameObject2D.Position, gameObject2D.BoundBox.Radius);

            Vector2 acceleration = Vector2.Zero;

            foreach (GameObject obj in objts)
            {
                if (obj == gameObject2D) continue;
                if (!TryGetGetMass(obj, out float m2)) continue;
                if (!CollisionDetection.Collide(elapsedMs, gameObject2D, obj, out Vector2? _)) continue;

                // Calc direction
                Vector2 pushDir = Vector2.Normalize(gameObject2D.Position - obj.Position);
                if (pushDir.IsNaN())
                    pushDir = Vector2.UnitX;

                MomentumConservation(m1, m2, gameObject2D.Velocity, obj.Velocity, out float v);
                acceleration += pushDir * v;
            }
            if (acceleration.X == 0 & acceleration.Y == 0) return;
            gameObject2D.MovingDirection += acceleration.NormalizedCopy();
            gameObject2D.Velocity *= acceleration.Length();
        }
    }
}

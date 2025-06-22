// CollisionDetection.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System;

namespace GameEngine.Gameplay
{
    public static class CollisionDetection
    {
        [Obsolete]
        public static bool Collide(GameTime gameTime, GameObject checkingObj, GameObject collidingObj, out Vector2? collidingPosition)
        {
            collidingPosition = null;

            // Check if Collision actually happened with the bounding box
            if (checkingObj.BoundBox.Intersects(collidingObj.BoundBox))
            {
                collidingPosition = checkingObj.Position;
                return true;
            }

            Vector2 futurePosition = checkingObj.Position + checkingObj.MovingDirection * (checkingObj.Velocity * gameTime.ElapsedGameTime.Milliseconds);
            float frameDistance = Vector2.Distance(checkingObj.Position, futurePosition);

            // Frame Movement Distance of Obj smaler than BoundBox Radius. No further check needet!
            if (frameDistance < checkingObj.BoundBox.Radius) return false;

            // Chech if Colliding Object is out of Frame Distance
            if (frameDistance < Vector2.Distance(checkingObj.Position, collidingObj.Position)) return false;

            // Object in Frame Distance
            float steps = frameDistance / checkingObj.BoundBox.Radius;
            MonoGame.Extended.CircleF predictBoundBox = checkingObj.BoundBox;

            for (float frameStep = 1; frameStep <= steps; frameStep += 1)
            {
                float step = frameDistance / steps * frameStep;
                predictBoundBox.Position = checkingObj.Position + checkingObj.MovingDirection * step;

                // Check if Collision happened at this step
                if (predictBoundBox.Intersects(collidingObj.BoundBox)) return false;
            }
            collidingPosition = predictBoundBox.Position;
            return true;
        }

        public static bool Collide(double elapsedMs, GameObject checkingObj, GameObject collidingObj, out Vector2? collidingPosition)
        {
            collidingPosition = null;

            // Check if Collision actually happened with the bounding box
            if (checkingObj.BoundBox.Intersects(collidingObj.BoundBox))
            {
                collidingPosition = checkingObj.Position;
                return true;
            }

            Vector2 futurePosition = checkingObj.Position + checkingObj.MovingDirection * (checkingObj.Velocity * (float)elapsedMs);
            float frameDistance = Vector2.Distance(checkingObj.Position, futurePosition);

            // Frame Movement Distance of Obj smaler than BoundBox Radius. No further check needet!
            if (frameDistance < checkingObj.BoundBox.Radius) return false;

            // Chech if Colliding Object is out of Frame Distance
            if (frameDistance < Vector2.Distance(checkingObj.Position, collidingObj.Position)) return false;

            // Object in Frame Distance
            float steps = frameDistance / checkingObj.BoundBox.Radius;
            MonoGame.Extended.CircleF predictBoundBox = checkingObj.BoundBox;

            for (float frameStep = 1; frameStep <= steps; frameStep += 1)
            {
                float step = frameDistance / steps * frameStep;
                predictBoundBox.Position = checkingObj.Position + checkingObj.MovingDirection * step;

                // Check if Collision happened at this step
                if (predictBoundBox.Intersects(collidingObj.BoundBox)) return false;
            }
            collidingPosition = predictBoundBox.Position;
            return true;
        }


        public static bool IsInside(GameObject checkingObj, GameObject collidingObj)
        {
            float distance = Vector2.Distance(checkingObj.Position, collidingObj.Position);
            return distance < checkingObj.BoundBox.Radius + collidingObj.BoundBox.Radius;
        }
    }
}

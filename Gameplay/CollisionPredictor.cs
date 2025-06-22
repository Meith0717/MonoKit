// CollisionPredictor.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Core;
using GameEngine.Extensions;
using Microsoft.Xna.Framework;

namespace GameEngine.Gameplay
{

    public enum MovementState
    {
        Expanding,
        Converging,
        MovingPerpendicular,
        None
    }

    public class CollisionPredictor
    {
        public static MovementState GetMovementState(Vector2 position, Vector2 targetPosition, Vector2 targetMovingDirection)
        {
            Vector2 dirToTarget = position.DirectionToVector2(targetPosition);
            float dotProduct = Vector2.Dot(dirToTarget, targetMovingDirection);
            return dotProduct switch
            {
                < 0 => MovementState.Converging,
                > 0 => MovementState.Expanding,
                0 => MovementState.MovingPerpendicular,
                _ => MovementState.None
            };
        }

        public static Vector2? PredictPosition(GameTime gameTime, Vector2 position, float speed, GameObject target)
        {
            if (target is null) return null;

            // Calculate the relative position of the target with respect to the shooter
            float distance = Vector2.Distance(position, target.Position);

            // Calculate the time of intersection using the relative motion equation
            float timeToIntersection = distance / (speed * gameTime.ElapsedGameTime.Milliseconds);

            // Calculate the future position of the target based on its velocity
            Vector2 pos = RNG.Random.NextVectorInCircle(target.BoundBox);
            Vector2 futureTargetPosition = pos + target.Velocity * gameTime.ElapsedGameTime.Milliseconds * target.MovingDirection * timeToIntersection;

            return futureTargetPosition;
        }
    }
}

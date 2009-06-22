﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DemoGame.Extensions;
using Microsoft.Xna.Framework;
using NetGore;
using NetGore.Extensions;

namespace DemoGame.Client
{
    /// <summary>
    /// Assists in smoothing out Entity movement by interpolating the position drawn.
    /// </summary>
    public class EntityInterpolator
    {
        /// <summary>
        /// The absolute value of the greatest velocity of the Entity.
        /// </summary>
        Vector2 _greatestVelocity;
        Vector2 _drawPosition;
        int _lastTime;

        /// <summary>
        /// Gets the position to use when drawing the Entity.
        /// </summary>
        public Vector2 DrawPosition
        {
            get { return _drawPosition.Round(); }
        }

        /// <summary>
        /// Updates the Entity's greatest velocity.
        /// </summary>
        /// <param name="currentVelocity">Current velocity.</param>
        void UpdateGreatestVelocity(Vector2 currentVelocity)
        {
            currentVelocity = currentVelocity.Abs();

            if (currentVelocity.X > _greatestVelocity.X)
                _greatestVelocity.X = currentVelocity.X;

            if (currentVelocity.Y > _greatestVelocity.Y)
                _greatestVelocity.Y = currentVelocity.Y;
        }

        /// <summary>
        /// Updates the drawing position interpolation.
        /// </summary>
        /// <param name="entity">Entity that this EntityInterpolator is for.</param>
        /// <param name="currentTime">Current game time.</param>
        public void Update(Entity entity, int currentTime)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            Vector2 position = entity.Position;
            UpdateGreatestVelocity(entity.Velocity);

            // Draw position and real position are already equal
            if (position == _drawPosition)
                return;

            int elapsedTime = currentTime - _lastTime;
            _lastTime = currentTime;

            // Get the velocity to use
            Vector2 velocity = entity.Velocity;
            if (velocity.X == 0)
            {
                velocity.X = _greatestVelocity.X;
                if (position.X < _drawPosition.X)
                    velocity.X *= -1;
            }
            if (velocity.Y == 0)
            {
                velocity.Y = _greatestVelocity.Y;
                if (position.Y < _drawPosition.Y)
                    velocity.Y *= -1;
            }

            // Get the new position
            Vector2 newPosition = _drawPosition + (velocity * elapsedTime);

            // Don't allow the draw position to exceed the real position
            if (_drawPosition.X > position.X && newPosition.X < position.X)
                newPosition.X = position.X;
            else if (_drawPosition.X < position.X && newPosition.X > position.X)
                newPosition.X = position.X;

            if (_drawPosition.Y > position.Y && newPosition.Y < position.Y)
                newPosition.Y = position.Y;
            else if (_drawPosition.Y < position.Y && newPosition.Y > position.Y)
                newPosition.Y = position.Y;

            // If we are too far out of sync, just jump to the real position
            Vector2 diff = newPosition - position;
            const float teleDiff = 25f;
            if (diff.X > teleDiff || diff.X < teleDiff || diff.Y > teleDiff || diff.Y < teleDiff)
                newPosition = position;

            // Set the new drawing position
            _drawPosition = newPosition;
        }
    }
}
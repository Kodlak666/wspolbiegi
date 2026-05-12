using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        private readonly object _ballsLock = new object();
        private readonly List<Data.IBall> _balls = new();
        private readonly double _boardWidth = 800;
        private readonly double _boardHeight = 400;

        public BusinessLogicImplementation() : this(null) { }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer ?? UnderneathLayerAPI.GetDataLayer();
        }

        public override void Dispose()
        {
            if (Disposed) throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            layerBellow.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null) throw new ArgumentNullException(nameof(upperLayerHandler));

            layerBellow.Start(numberOfBalls, (startingPosition, databall) =>
            {
                lock (_ballsLock)
                {
                    if (!_balls.Contains(databall))
                    {
                        _balls.Add(databall);
                        databall.NewPositionNotification += (sender, pos) => CheckCollisions(sender, pos);
                    }
                }
                upperLayerHandler(new Position(startingPosition.x, startingPosition.y), new Ball(databall));
            });
        }

        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;

        private void CheckCollisions(object? sender, Data.IVector newPosition)
        {
            if (sender is not Data.IBall currentBall) return;
            CheckWallCollisions(currentBall);

            lock (_ballsLock)
            {
                foreach (var otherBall in _balls)
                {
                    if (currentBall == otherBall) continue;
                    CheckBallCollision(currentBall, otherBall);
                }
            }
        }

        private void CheckWallCollisions(Data.IBall ball)
        {
            var pos = ball.Position;
            var vel = ball.Velocity;
            double newVx = vel.x;
            double newVy = vel.y;
            bool bounced = false;

            if (pos.x <= 0) { newVx = Math.Abs(vel.x); bounced = true; }
            else if (pos.x >= _boardWidth - ball.Diameter) { newVx = -Math.Abs(vel.x); bounced = true; }

            if (pos.y <= 0) { newVy = Math.Abs(vel.y); bounced = true; }
            else if (pos.y >= _boardHeight - ball.Diameter) { newVy = -Math.Abs(vel.y); bounced = true; }

            if (bounced) ball.Velocity = new Data.Vector(newVx, newVy);
        }

        private void CheckBallCollision(Data.IBall b1, Data.IBall b2)
        {
            object firstLock = b1.GetHashCode() < b2.GetHashCode() ? b1 : b2;
            object secondLock = b1.GetHashCode() < b2.GetHashCode() ? b2 : b1;

            lock (firstLock)
            {
                lock (secondLock)
                {
                    var p1 = b1.Position;
                    var p2 = b2.Position;
                    double center1X = p1.x + b1.Diameter / 2;
                    double center1Y = p1.y + b1.Diameter / 2;
                    double center2X = p2.x + b2.Diameter / 2;
                    double center2Y = p2.y + b2.Diameter / 2;

                    double dx = center1X - center2X;
                    double dy = center1Y - center2Y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    double minDistance = (b1.Diameter / 2) + (b2.Diameter / 2);

                    if (distance < minDistance)
                    {
                        var v1 = b1.Velocity;
                        var v2 = b2.Velocity;

                        double relativeVelocityX = v1.x - v2.x;
                        double relativeVelocityY = v1.y - v2.y;
                        if ((relativeVelocityX * dx + relativeVelocityY * dy) > 0) return;
                        
                        double m1 = b1.Mass;
                        double m2 = b2.Mass;

                        double commonPart = 2 * (v1.x * dx + v1.y * dy - v2.x * dx - v2.y * dy) / ((m1 + m2) * (dx * dx + dy * dy));

                        double v1x = v1.x - commonPart * m2 * dx;
                        double v1y = v1.y - commonPart * m2 * dy;
                        double v2x = v2.x + commonPart * m1 * dx;
                        double v2y = v2.y + commonPart * m1 * dy;

                        b1.Velocity = new Data.Vector(v1x, v1y);
                        b2.Velocity = new Data.Vector(v2x, v2y);
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }
    }
}
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

            var velocitySnapshots = _balls.ToDictionary(b => b, b => b.Velocity);

            double accumulatedDx = 0;
            double accumulatedDy = 0;

            lock (_ballsLock)
            {
                var v1Snapshot = velocitySnapshots[currentBall];

                foreach (var otherBall in _balls)
                {
                    if (currentBall == otherBall) continue;

                    var v2Snapshot = velocitySnapshots[otherBall];
                    var impulses = GetImpulse(currentBall, v1Snapshot, otherBall, v2Snapshot);

                    accumulatedDx += impulses.dv1.x;
                    accumulatedDy += impulses.dv1.y;
                }

                if (accumulatedDx != 0 || accumulatedDy != 0)
                {
                    currentBall.Velocity = new Data.Vector(v1Snapshot.x + accumulatedDx, v1Snapshot.y + accumulatedDy);
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

        private (Data.Vector dv1, Data.Vector dv2) GetImpulse(Data.IBall b1, Data.IVector v1, Data.IBall b2, Data.IVector v2)
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

                    double relativeVelocityX = v1.x - v2.x;
                    double relativeVelocityY = v1.y - v2.y;
                    double dot = relativeVelocityX * dx + relativeVelocityY * dy;

                    if (distance <= minDistance && dot <= 0)
                    {
                        double commonPart = 2 * dot / ((b1.Mass + b2.Mass) * (dx * dx + dy * dy));
                        return (
                                new Data.Vector(-commonPart * b2.Mass * dx, -commonPart * b2.Mass * dy),
                                new Data.Vector(commonPart * b1.Mass * dx, commonPart * b1.Mass * dy)
                               );
                    }

                }
            }

            return (new Data.Vector(0, 0), new Data.Vector(0, 0));
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }
    }
}

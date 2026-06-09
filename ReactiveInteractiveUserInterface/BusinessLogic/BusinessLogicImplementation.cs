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
        private readonly double _boardWidth = 420;
        private readonly double _boardHeight = 400;
        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;

        public BusinessLogicImplementation() : this(null) { }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer ?? UnderneathLayerAPI.GetDataLayer();
        }

        // NOWE: Przekazywanie ruchu myszki niżej do warstwy Danych
        public override void UpdateMousePosition(double x, double y)
        {
            layerBellow.UpdateMousePosition(x, y);
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

            lock (_ballsLock)
            {
                _balls.Clear();
            }

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
            bool velocityChanged = false;

            // Hard Limit: Ograniczamy maksymalną prędkość, żeby kule nie ignorowały ścian
            double maxSpeed = 1000.0;
            if (newVx > maxSpeed) { newVx = maxSpeed; velocityChanged = true; }
            if (newVx < -maxSpeed) { newVx = -maxSpeed; velocityChanged = true; }
            if (newVy > maxSpeed) { newVy = maxSpeed; velocityChanged = true; }
            if (newVy < -maxSpeed) { newVy = -maxSpeed; velocityChanged = true; }

            if (pos.x <= 0) { newVx = Math.Abs(newVx); velocityChanged = true; }
            else if (pos.x >= _boardWidth - ball.Diameter) { newVx = -Math.Abs(newVx); velocityChanged = true; }

            if (pos.y <= 0) { newVy = Math.Abs(newVy); velocityChanged = true; }
            else if (pos.y >= _boardHeight - ball.Diameter) { newVy = -Math.Abs(newVy); velocityChanged = true; }

            if (velocityChanged)
            {
                ball.Velocity = new Data.Vector(newVx, newVy);
            }
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
                    double distanceSquared = dx * dx + dy * dy;

                    if (distanceSquared < 1.0) return;
                    double minDistance = (b1.Diameter / 2) + (b2.Diameter / 2);

                    if (distanceSquared < minDistance * minDistance)
                    {
                        // Wysłanie logów o kolizji do warstwy Danych
                        layerBellow.LogDiagnosticData($"Collision: Ball {b1.GetHashCode()} bounced off Ball {b2.GetHashCode()}");
                        layerBellow.LogDiagnosticData($"Collision: Ball {b2.GetHashCode()} bounced off Ball {b1.GetHashCode()}");

                        var v1 = b1.Velocity;
                        var v2 = b2.Velocity;

                        // Zabezpieczenie przed wielokrotnym zderzeniem!
                        // Jeśli kule już się od siebie oddalają, przerywamy obliczenia.
                        double relativeVelocityX = v1.x - v2.x;
                        double relativeVelocityY = v1.y - v2.y;
                        if ((relativeVelocityX * dx + relativeVelocityY * dy) > 0) return;

                        double m1 = b1.Mass;
                        double m2 = b2.Mass;

                        if (double.IsPositiveInfinity(m1) || double.IsPositiveInfinity(m2))
                        {
                            Data.IBall normalBall = double.IsPositiveInfinity(m1) ? b2 : b1;
                            Data.IBall mouseBall = double.IsPositiveInfinity(m1) ? b1 : b2;

                            // Obliczamy wektor kierunkowy od myszki do kulki
                            double pushX = normalBall.Position.x - mouseBall.Position.x;
                            double pushY = normalBall.Position.y - mouseBall.Position.y;

                            // Normalizacja wektora (żeby długość wynosiła 1)
                            double distance = Math.Sqrt(pushX * pushX + pushY * pushY);
                            if (distance == 0) distance = 1;

                            // Ustawiamy stałą, szybką prędkość po uderzeniu myszką
                            double kickForce = 600.0;

                            normalBall.Velocity = new Data.Vector(
                                (pushX / distance) * kickForce,
                                (pushY / distance) * kickForce
                            );
                        }
                        else
                        {
                            // Standardowa fizyka zderzeń dla zwykłych kulek
                            double commonPart = 2 * (relativeVelocityX * dx + relativeVelocityY * dy) / ((m1 + m2) * distanceSquared);

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
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }
    }
}
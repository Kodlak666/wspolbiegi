//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        private bool Disposed = false;
        private List<Ball> BallsList = new List<Ball>();
        private CancellationTokenSource? _cancelTokenSource;
        private readonly object _lock = new object();

        private readonly double _boardWidth = 800;
        private readonly double _boardHeight = 400;

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            BallsList.Clear();
            _cancelTokenSource = new CancellationTokenSource();
            Random random = new Random();

            for (int i = 0; i < numberOfBalls; i++)
            {
                double ballDiameter = 30;
                double initialX = random.NextDouble() * (_boardWidth - ballDiameter);
                double initialY = random.NextDouble() * (_boardHeight - ballDiameter);
                Vector startingPosition = new Vector(initialX, initialY);

                double velX = (random.NextDouble() - 0.5) * 10;
                double velY = (random.NextDouble() - 0.5) * 10;
                Vector initialVelocity = new Vector(velX, velY);

                Ball newBall = new Ball(startingPosition, initialVelocity, ballDiameter);
                BallsList.Add(newBall);

                upperLayerHandler(startingPosition, newBall);
            }
            Task.Run(() => SimulationLoop(_cancelTokenSource.Token));
        }

        #endregion DataAbstractAPI

        #region Physics Simulation

        private async Task SimulationLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                lock (_lock)
                {
                    foreach (var ball in BallsList)
                    {
                        double nextX = ball.Position.x + ball.Velocity.x;
                        double nextY = ball.Position.y + ball.Velocity.y;

                        double currentVelX = ball.Velocity.x;
                        double currentVelY = ball.Velocity.y;

                        if (nextX <= 0 || nextX + ball.Diameter >= _boardWidth - 8)
                        {
                            currentVelX = -currentVelX;
                        }

                        if (nextY <= 0 || nextY + ball.Diameter / 2 >= _boardHeight - 4)
                        {
                            currentVelY = -currentVelY;
                        }

                        ball.Velocity = new Vector(currentVelX, currentVelY);
                        ball.Move();
                    }
                }

                await Task.Delay(16, token);
            }
        }

        #endregion Physics Simulation

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    _cancelTokenSource?.Cancel();
                    BallsList.Clear();
                }
                Disposed = true;
            }
        }

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            returnBallsList(BallsList);
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            returnNumberOfBalls(BallsList.Count);
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}
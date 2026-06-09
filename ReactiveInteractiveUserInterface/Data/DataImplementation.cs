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
        private readonly List<Ball> _balls = new();
        private readonly Random _random = new();
        private CancellationTokenSource? _cancelSource;

        // Podpięcie naszego asynchronicznego loggera
        private readonly AsyncLogger _logger = new AsyncLogger();

        private readonly double _boardWidth = 420;
        private readonly double _boardHeight = 400;

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            _cancelSource?.Cancel();
            _balls.Clear();
            _cancelSource = new CancellationTokenSource();

            double speed = 200;

            for (int i = 0; i < numberOfBalls; i++)
            {
                double diameter = _random.NextDouble() * (30.0 - 10.0) + 10.0;
                // Kulka-myszka (i == 0) ma nieskończoną masę z perspektywy zderzeń
                double mass = (i == 0) ? double.PositiveInfinity : _random.NextDouble() * (5.0 - 1.0) + 1.0;

                double x = _random.NextDouble() * (_boardWidth - diameter);
                double y = _random.NextDouble() * (_boardHeight - diameter);

                // Kulka-myszka nie ma początkowej prędkości własnej
                double vx = (i == 0) ? 0 : (_random.NextDouble() * speed) - (speed / 2);
                double vy = (i == 0) ? 0 : (_random.NextDouble() * speed) - (speed / 2);

                if (i != 0 && Math.Abs(vx) < 0.1) vx = 1.0;
                if (i != 0 && Math.Abs(vy) < 0.1) vy = 1.0;

                Ball newBall = new Ball(
                    new Vector(x, y),
                    new Vector(vx, vy),
                    diameter,
                    mass
                );

                _balls.Add(newBall);
                upperLayerHandler(newBall.Position, newBall);

                // Przekazujemy flagę, czy to kulka myszki
                Task.Run(() => BallMovementLoop(newBall, _cancelSource.Token, isMouseBall: i == 0));
            }
        }

        // Nowa metoda łapiąca pozycję myszki i przypisująca ją do kuli nr 0
        public override void UpdateMousePosition(double x, double y)
        {
            if (_balls.Count > 0)
            {
                _balls[0].SetPosition(x, y);
            }
        }

        private async Task BallMovementLoop(Ball ball, CancellationToken token, bool isMouseBall)
        {
            int targetDelay = 16;
            Stopwatch stopwatch = new Stopwatch();

            while (!token.IsCancellationRequested)
            {
                stopwatch.Restart();

                // Kulka myszkowa nie porusza się sama według wektora prędkości
                if (!isMouseBall)
                {
                    ball.Move(stopwatch.ElapsedMilliseconds * 0.001f);
                }

                // Logujemy diagnostykę do pliku
                _logger.Log($"Ball {ball.GetHashCode()} moved to X:{ball.Position.x:F2} Y:{ball.Position.y:F2}");

                stopwatch.Stop();
                int elapsed = (int)stopwatch.ElapsedMilliseconds;
                int remainingDelay = targetDelay - elapsed;

                if (remainingDelay > 0)
                {
                    await Task.Delay(remainingDelay, token).ContinueWith(_ => { });
                }
                else
                {
                    await Task.Yield();
                }
            }
        }

        public override void Dispose()
        {
            if (Disposed) throw new ObjectDisposedException(nameof(DataImplementation));
            _cancelSource?.Cancel();
            _cancelSource?.Dispose();
            _cancelSource = null;
            _balls.Clear();
            _logger.Dispose(); // Pamiętamy o zabiciu loggera!
            Disposed = true;
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnCount) => returnCount(_balls.Count);

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnStatus) => returnStatus(Disposed);

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnList) => returnList(_balls);
    }
}

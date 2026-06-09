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

            double speed = 800;

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
                Task.Run(() => BallMovementLoop(newBall, _cancelSource.Token));
            }
        }
        public override void LogDiagnosticData(string message)
        {
            _logger.Log(message);
        }

        // Nowa metoda łapiąca pozycję myszki i przypisująca ją do kuli nr 0
        public override void UpdateMousePosition(double x, double y)
        {
            if (_balls.Count > 0)
            {
                _balls[0].SetPosition(x, y);
            }
        }

        private async Task BallMovementLoop(Ball ball, CancellationToken token)
        {
            int targetDelay = 16;

            // Uruchamiamy stoper RAZ i już go nie restartujemy
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Zmienne do śledzenia czasu
            long lastTick = stopwatch.ElapsedMilliseconds;
            long nextTick = lastTick;

            while (!token.IsCancellationRequested)
            {
                long currentTick = stopwatch.ElapsedMilliseconds;

                // Obliczamy PRAWDZIWY deltatime (różnica czasu między klatkami w sekundach)
                double deltaTime = (currentTick - lastTick) * 0.001;

                // Zabezpieczenie: w pierwszej klatce czas może być 0, więc dajemy domyślne 16ms
                if (deltaTime <= 0) deltaTime = 0.016;

                lastTick = currentTick;

                // Wywołujemy Move dla KAŻDEJ kulki. 
                // Kulka-myszka ma prędkość 0, więc się nie ruszy z miejsca, 
                // ale WYŚLE EVENT do testu jednostkowego!
                ball.Move(deltaTime);

                _logger.Log($"Ball {ball.GetHashCode()} moved to X:{ball.Position.x:F2} Y:{ball.Position.y:F2}");

                // Deterministyczne wyrównywanie czasu (żeby test na 125 ruchów przechodził)
                nextTick += targetDelay;
                long delay = nextTick - stopwatch.ElapsedMilliseconds;

                if (delay > 0)
                {
                    await Task.Delay((int)delay, token).ContinueWith(_ => { });
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

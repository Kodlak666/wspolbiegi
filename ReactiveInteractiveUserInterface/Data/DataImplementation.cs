using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        private bool Disposed = false;
        private readonly List<Ball> _balls = new();
        private readonly Random _random = new();
        private CancellationTokenSource? _cancelSource;

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
                double mass = _random.NextDouble() * (5.0 - 1.0) + 1.0;

                double x = _random.NextDouble() * (_boardWidth - diameter);
                double y = _random.NextDouble() * (_boardHeight - diameter);

                double vx = (_random.NextDouble() * speed) - (speed / 2);
                double vy = (_random.NextDouble() * speed) - (speed / 2);

                if (Math.Abs(vx) < 0.1) vx = 1.0;
                if (Math.Abs(vy) < 0.1) vy = 1.0;

                Ball newBall = new Ball(
                    new Vector(x, y),
                    new Vector(vx, vy),
                    diameter,
                    mass
                );

                _balls.Add(newBall);

                upperLayerHandler(newBall.Position, newBall);

                Task.Run(() => BallMovementLoop(newBall, _cancelSource.Token));
            }
        }

        private async Task BallMovementLoop(Ball ball, CancellationToken token)
        {
            Stopwatch watch = new Stopwatch();

            while (!token.IsCancellationRequested)
            {
                watch.Stop();
                ball.Move(watch.ElapsedMilliseconds * 0.001f);
                watch.Restart();
                await Task.Delay(16, token).ContinueWith(_ => { });
            }
        }

        public override void Dispose()
        {
            if (Disposed) throw new ObjectDisposedException(nameof(DataImplementation));
            _cancelSource?.Cancel();
            _cancelSource?.Dispose();
            _cancelSource = null;
            _balls.Clear();
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

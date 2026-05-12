using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        private bool Disposed = false;
        private readonly List<Ball> _balls = new();
        private readonly Random _random = new();
        private CancellationTokenSource? _cancelSource;

        private readonly double _boardWidth = 800;
        private readonly double _boardHeight = 400;

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            _cancelSource = new CancellationTokenSource();

            for (int i = 0; i < numberOfBalls; i++)
            {
                double diameter = _random.NextDouble() * (30.0 - 10.0) + 10.0;
                double mass = _random.NextDouble() * (5.0 - 1.0) + 1.0;

                double x = _random.NextDouble() * (_boardWidth - diameter);
                double y = _random.NextDouble() * (_boardHeight - diameter);

                double vx = (_random.NextDouble() * 4.0) - 2.0;
                double vy = (_random.NextDouble() * 4.0) - 2.0;

                if (vx == 0) vx = 1.0;
                if (vy == 0) vy = 1.0;

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
            while (!token.IsCancellationRequested)
            {
                ball.Move();
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

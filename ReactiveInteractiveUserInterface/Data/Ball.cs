using System;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        private readonly object _lock = new object();
        private Vector _position;
        private IVector _velocity;

        public double Diameter { get; init; }
        public double Mass { get; init; }

        public IVector Position
        {
            get { lock (_lock) { return _position; } }
        }

        public IVector Velocity
        {
            get { lock (_lock) { return _velocity; } }
            set { lock (_lock) { _velocity = value; } }
        }

        public event EventHandler<IVector>? NewPositionNotification;

        internal Ball(Vector initialPosition, Vector initialVelocity, double diameter, double mass)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
            Diameter = diameter;
            Mass = mass;
        }

        internal void Move()
        {
            lock (_lock)
            {
                _position = new Vector(_position.x + _velocity.x, _position.y + _velocity.y);
            }
            NewPositionNotification?.Invoke(this, Position);
        }

        // NOWE: Metoda do twardego nadpisywania pozycji przez myszkę
        public void SetPosition(double x, double y)
        {
            lock (_lock)
            {
                _position = new Vector(x, y);
            }
            // Ważne: powiadamiamy UI o nowej pozycji z myszki
            NewPositionNotification?.Invoke(this, Position);
        }
    }
}
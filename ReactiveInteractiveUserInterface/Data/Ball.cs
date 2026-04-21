//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
  internal class Ball : IBall
  {
    #region ctor
        public double Diameter { get; init; }
        internal Vector Position { get; private set; }
        internal Ball(Vector initialPosition, Vector initialVelocity, double diameter)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
            Diameter = diameter;
        }

    #endregion ctor

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;

    public IVector Velocity { get; set; }

    #endregion IBall

    #region private

    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

    internal void Move()
    {
      Position = new Vector(Position.x + Velocity.x, Position.y + Velocity.y);
      RaiseNewPositionChangeNotification();
    }

    #endregion private
  }
}
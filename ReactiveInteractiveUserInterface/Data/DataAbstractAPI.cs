using System;

namespace TP.ConcurrentProgramming.Data
{
    public abstract class DataAbstractAPI : IDisposable
    {
        public static DataAbstractAPI GetDataLayer()
        {
            return modelInstance.Value;
        }

        public abstract void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler);

        public abstract void UpdateMousePosition(double x, double y);
        public abstract void LogDiagnosticData(string message);

        public abstract void Dispose();

        private static Lazy<DataAbstractAPI> modelInstance = new Lazy<DataAbstractAPI>(() => new DataImplementation());
    }

    public interface IVector
    {
        double x { get; init; }
        double y { get; init; }
    }

    public interface IBall
    {
        event EventHandler<IVector> NewPositionNotification;
        IVector Velocity { get; set; }
        IVector Position { get; }
        double Diameter { get; }
        double Mass { get; }
        void SetPosition(double x, double y);
    }
}
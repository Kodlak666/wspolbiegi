using System;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    public abstract class BusinessLogicAbstractAPI : IDisposable
    {
        // Wstrzykiwanie Zależności (DI) - możemy przekazać fake'ową warstwę Danych
        public static BusinessLogicAbstractAPI GetBusinessLogicLayer(Data.DataAbstractAPI dataLayer = null)
        {
            return new BusinessLogicImplementation(dataLayer ?? Data.DataAbstractAPI.GetDataLayer());
        }

        public static Dimensions GetDimensions => new Dimensions(800, 400, 10);

        public abstract void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler);

        // POPRAWKA: Metoda jest teraz poprawnie wewnątrz klasy API (a nie w interfejsie IBall)
        public abstract void UpdateMousePosition(double x, double y);

        public abstract void Dispose();
    }

    public record Dimensions(double BallDimension, double TableHeight, double TableWidth);

    public interface IPosition
    {
        double x { get; init; }
        double y { get; init; }
    }

    public interface IBall
    {
        event EventHandler<IPosition> NewPositionNotification;
        double Diameter { get; }
    }
}
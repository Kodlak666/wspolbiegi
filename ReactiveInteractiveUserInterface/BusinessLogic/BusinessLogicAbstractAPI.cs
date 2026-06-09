using System;

namespace TP.ConcurrentProgramming.BusinessLogic
{
  public abstract class BusinessLogicAbstractAPI : IDisposable
  {
    #region Layer Factory

    public static BusinessLogicAbstractAPI GetBusinessLogicLayer()
    {
      return modelInstance.Value;
    }

        #endregion Layer Factory

        #region Layer API

        public static Dimensions GetDimensions => new Dimensions(800, 400, 10);

    public abstract void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler);
    public abstract void UpdateMousePosition(double x, double y);

    #region IDisposable

    public abstract void Dispose();

    #endregion IDisposable

    #endregion Layer API

    #region private

    private static Lazy<BusinessLogicAbstractAPI> modelInstance = new Lazy<BusinessLogicAbstractAPI>(() => new BusinessLogicImplementation());

    #endregion private
  }
  /// <summary>
  /// Immutable type representing table dimensions
  /// </summary>
  /// <param name="BallDimension"></param>
  /// <param name="TableHeight"></param>
  /// <param name="TableWidth"></param>
  /// <remarks>
  /// Must be abstract
  /// </remarks>
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

//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//_____________________________________________________________________________________________________________________________________
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BusinessLogicUnitTest
    {
        [TestMethod]
        public void BallMoveTestMethod()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            Ball newInstance = new(dataBallFixture);
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); Assert.IsNotNull(position); numberOfCallBackCalled++; };

            dataBallFixture.Move();

            Assert.AreEqual<int>(1, numberOfCallBackCalled);
            Assert.AreEqual(2.5, dataBallFixture.Mass);
            Assert.AreEqual(20.0, dataBallFixture.Diameter);
        }

        [TestMethod]
        public void BusinessLogicCollisionTestWithDI()
        {
            MockDataAPI mockDataLayer = new MockDataAPI();

            BusinessLogicImplementation logic = new BusinessLogicImplementation(mockDataLayer);

            int ballsPassedToUI = 0;
            logic.Start(2, (pos, logicBall) => {
                ballsPassedToUI++;
            });

            Assert.AreEqual(2, ballsPassedToUI);
            logic.Dispose();
        }

        [TestMethod]
        public void CornerCollisionTestMethod()
        {
            DataBallFixture ball = new DataBallFixture
            {
                Position = new VectorFixture(0, 0),
                Velocity = new VectorFixture(-2.0, -2.0),
                Diameter = 20.0
            };

            MockDataAPI mockApi = new MockDataAPI(new List<Data.IBall> { ball });
            BusinessLogicImplementation logic = new BusinessLogicImplementation(mockApi);

            logic.Start(1, (pos, b) => { });

            ball.Move();

            Assert.AreEqual(2.0, ball.Velocity.x, "Kula powinna odbić się od lewej ściany");
            Assert.AreEqual(2.0, ball.Velocity.y, "Kula powinna odbić się od górnej ściany");
        }

        [TestMethod]
        public void ThreeBallsCollisionTestMethod()
        {
            DataBallFixture ball1 = new DataBallFixture
            {
                Position = new VectorFixture(50, 50),
                Velocity = new VectorFixture(1.0, 0.0), 
                Diameter = 20.0,
                Mass = 2.5
            };
            DataBallFixture ball2 = new DataBallFixture
            {
                Position = new VectorFixture(65, 50), 
                Velocity = new VectorFixture(-1.0, 0.0), 
                Diameter = 20.0,
                Mass = 2.5
            };
            DataBallFixture ball3 = new DataBallFixture
            {
                Position = new VectorFixture(57, 65),
                Velocity = new VectorFixture(0.0, -1.0),
                Diameter = 20.0,
                Mass = 2.5
            };
            MockDataAPI mockApi = new MockDataAPI(new List<Data.IBall> { ball1, ball2, ball3 });
            BusinessLogicImplementation logic = new BusinessLogicImplementation(mockApi);
            logic.Start(3, (pos, b) => { });
            Parallel.Invoke(
                () => ball1.Move(),
                () => ball2.Move(),
                () => ball3.Move()
            );
            Assert.IsTrue(ball1.Velocity.x < 0, $"Ball1 powinien odbić się w lewo. Obecna prędkość: {ball1.Velocity.x}");
            Assert.IsTrue(ball2.Velocity.x > 0, $"Ball2 powinien odbić się w prawo. Obecna prędkość: {ball2.Velocity.x}");
            Assert.IsTrue(ball3.Velocity.y > 0, $"Ball3 powinien zmienić kierunek pionowy. Obecna prędkość: {ball3.Velocity.y}");
        }

        #region testing instrumentation

        private class MockDataAPI : Data.DataAbstractAPI
        {
            private List<Data.IBall> _predefinedBalls;

            public MockDataAPI(List<Data.IBall> predefinedBalls = null)
            {
                _predefinedBalls = predefinedBalls ?? new List<Data.IBall>();
            }

            public override void Start(int numberOfBalls, Action<Data.IVector, Data.IBall> upperLayerHandler)
            {
                if (_predefinedBalls.Count > 0)
                {
                    foreach (var mockBall in _predefinedBalls)
                    {
                        upperLayerHandler(mockBall.Position, mockBall);
                    }
                }
                else
                {
                    for (int i = 0; i < numberOfBalls; i++)
                    {
                        DataBallFixture mockBall = new DataBallFixture();
                        upperLayerHandler(mockBall.Position, mockBall);
                    }
                }
            }

            public override void Dispose() { }
        }

        private class DataBallFixture : Data.IBall
        {
            public Data.IVector Velocity { get; set; } = new VectorFixture(1.0, 1.0);
            public Data.IVector Position { get; set; } = new VectorFixture(0.0, 0.0);
            public double Diameter { get; init; } = 20.0;
            public double Mass { get; init; } = 2.5;

            public event EventHandler<Data.IVector>? NewPositionNotification;

            internal void Move()
            {
                NewPositionNotification?.Invoke(this, new VectorFixture(0.0, 0.0));
            }
        }

        private class VectorFixture : Data.IVector
        {
            internal VectorFixture(double X, double Y)
            {
                x = X; y = Y;
            }

            public double x { get; init; }
            public double y { get; init; }
        }

        #endregion testing instrumentation
    }
}
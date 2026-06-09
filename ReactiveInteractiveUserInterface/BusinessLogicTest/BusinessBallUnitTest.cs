//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//_____________________________________________________________________________________________________________________________________
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TP.ConcurrentProgramming.BusinessLogic;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void MoveTestMethod()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            Ball newInstance = new(dataBallFixture);
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) =>
            {
                Assert.IsNotNull(sender);
                Assert.IsNotNull(position);
                numberOfCallBackCalled++;
            };

            dataBallFixture.Move();
            Assert.AreEqual<int>(1, numberOfCallBackCalled);
            Assert.AreEqual(dataBallFixture.Diameter, newInstance.Diameter);
        }

        #region testing instrumentation

        private class DataBallFixture : Data.IBall
        {
            public Data.IVector Velocity { get; set; } = new VectorFixture(0, 0);
            public Data.IVector Position { get; set; } = new VectorFixture(0, 0);
            public double Diameter { get; init; } = 20.0;
            public double Mass { get; init; } = 1.0;

            public event EventHandler<Data.IVector>? NewPositionNotification;

            internal void Move()
            {
                NewPositionNotification?.Invoke(this, Position);
            }
            public void SetPosition(double x, double y)
            {
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
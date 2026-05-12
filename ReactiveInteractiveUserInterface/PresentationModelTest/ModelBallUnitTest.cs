//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//_____________________________________________________________________________________________________________________________________
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TP.ConcurrentProgramming.BusinessLogic;

namespace TP.ConcurrentProgramming.Presentation.Model.Test
{
    [TestClass]
    public class ModelBallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            double testDiameter = 20.0;
            ModelBall ball = new ModelBall(0.0, 0.0, new BusinessLogicIBallFixture(testDiameter), 1.0, 1.0);
            Assert.AreEqual<double>(0.0, ball.Top);
            Assert.AreEqual<double>(0.0, ball.Left);
            Assert.AreEqual<double>(testDiameter, ball.Diameter);
        }

        [TestMethod]
        public void PositionChangeNotificationTestMethod()
        {
            int notificationCounter = 0;
            ModelBall ball = new ModelBall(0, 0.0, new BusinessLogicIBallFixture(20.0), 1.0, 1.0);
            ball.PropertyChanged += (sender, args) => notificationCounter++;

            Assert.AreEqual(0, notificationCounter);
            ball.SetLeft(1.0);
            Assert.AreEqual<int>(1, notificationCounter);
            Assert.AreEqual<double>(1.0, ball.Left);
            Assert.AreEqual<double>(0.0, ball.Top);

            ball.SettTop(1.0);
            Assert.AreEqual(2, notificationCounter);
            Assert.AreEqual<double>(1.0, ball.Left);
            Assert.AreEqual<double>(1.0, ball.Top);
        }

        #region testing instrumentation

        private class BusinessLogicIBallFixture : BusinessLogic.IBall
        {
            public event EventHandler<IPosition>? NewPositionNotification;
            public double Diameter { get; private set; }

            public BusinessLogicIBallFixture(double diameter)
            {
                Diameter = diameter;
            }
            public void Dispose() { }
        }

        #endregion testing instrumentation
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            Vector testingVector = new Vector(0.0, 0.0);
            Ball newInstance = new Ball(testingVector, testingVector, 20.0, 1.0);

            Assert.AreEqual(20.0, newInstance.Diameter);
            Assert.AreEqual(1.0, newInstance.Mass);
        }

        [TestMethod]
        public void MoveTestMethod()
        {
            Vector initialPosition = new Vector(10.0, 10.0);
            Vector velocity = new Vector(2.0, 2.0);
            Ball newInstance = new Ball(initialPosition, velocity, 20.0, 1.0);

            IVector currentPosition = new Vector(0.0, 0.0);
            int numberOfCallBackCalled = 0;

            newInstance.NewPositionNotification += (sender, position) =>
            {
                Assert.IsNotNull(sender);
                currentPosition = position;
                numberOfCallBackCalled++;
            };

            newInstance.Move(1);

            Assert.AreEqual<int>(1, numberOfCallBackCalled);    
            Assert.AreEqual(12.0, currentPosition.x);
            Assert.AreEqual(12.0, currentPosition.y);
        }
    }
}

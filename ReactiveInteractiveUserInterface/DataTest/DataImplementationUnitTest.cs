//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class DataImplementationUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                IEnumerable<IBall>? ballsList = null;
                newInstance.CheckBallsList(x => ballsList = x);
                Assert.IsNotNull(ballsList);
                int numberOfBalls = 0;
                newInstance.CheckNumberOfBalls(x => numberOfBalls = x);
                Assert.AreEqual<int>(0, numberOfBalls);
            }
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataImplementation newInstance = new DataImplementation();
            bool newInstanceDisposed = false;
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsFalse(newInstanceDisposed, $"isNewInstanceDisposed: {newInstanceDisposed}");
            newInstance.Dispose();
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsTrue(newInstanceDisposed);
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
        }

        [TestMethod]
        public void StartTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                int numberOfCallbackInvoked = 0;
                int numberOfBalls2Create = 10;
                newInstance.Start(
                  numberOfBalls2Create,
                  (startingPosition, ball) =>
                  {
                      numberOfCallbackInvoked++;
                      Assert.IsTrue(startingPosition.x >= 0);
                      Assert.IsTrue(startingPosition.y >= 0);
                      Assert.IsNotNull(ball);
                  });
                Assert.IsTrue(numberOfCallbackInvoked >= numberOfBalls2Create);
            }
        }
        [TestMethod]
        public async Task AsyncMovementTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                int moveCount = 0;
                TaskCompletionSource<bool> ballCreated = new TaskCompletionSource<bool>();

                newInstance.Start(1, (pos, ball) =>
                {
                    ball.NewPositionNotification += (s, p) => moveCount++;
                    ballCreated.SetResult(true);
                });

                await ballCreated.Task;
                await Task.Delay(200);

                Assert.IsTrue(moveCount >= 8, $"Kula porusza się zbyt wolno! Oczekiwano ok. 12 ruchów, było: {moveCount}");
                Assert.IsTrue(moveCount <= 16, $"Kula porusza się zbyt szybko! Oczekiwano ok. 12 ruchów, było: {moveCount}");
            }
        }
    }
}

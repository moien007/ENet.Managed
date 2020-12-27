
using ENet.Managed.Internal.Threading;

using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    class FiloSemaphoreTests
    {
        [Test]
        public void TestAsyncLocking()
        {
            var sema = new FiloSemaphore();

            Assert.IsFalse(sema.IsLocked);

            sema.Lock();

            Assert.IsTrue(sema.IsLocked);
            Assert.AreEqual(0, sema.WaitersCount);

            var waiter1 = sema.LockAsync();
            var waiter2 = sema.LockAsync();
            var waiter3 = sema.LockAsync();

            Assert.IsTrue(sema.IsLocked);
            Assert.AreEqual(3, sema.WaitersCount);
            Assert.IsFalse(waiter1.IsCompleted);
            Assert.IsFalse(waiter2.IsCompleted);
            Assert.IsFalse(waiter3.IsCompleted);
            sema.Release();

            Assert.IsTrue(sema.IsLocked);
            Assert.IsTrue(waiter1.IsCompleted);
            Assert.IsFalse(waiter2.IsCompleted);
            Assert.IsFalse(waiter3.IsCompleted);
            sema.Release();

            Assert.IsTrue(sema.IsLocked);
            Assert.AreEqual(1, sema.WaitersCount);
            Assert.IsTrue(waiter2.IsCompleted);
            Assert.IsFalse(waiter3.IsCompleted);
            sema.Release();

            Assert.IsTrue(sema.IsLocked);
            Assert.AreEqual(0, sema.WaitersCount);
            Assert.IsTrue(waiter3.IsCompleted);
            sema.Release();

            Assert.IsFalse(sema.IsLocked);
        }
    }
}

using System.Net;

using NUnit.Framework;

using static ENet.Managed.ManagedENetHelpers;

#pragma warning disable CS0618 // Type or member is obsolete

namespace UnitTests
{
    [TestFixture]
    public class ManagedENetHelpersTests
    {
        [Test]
        public void TryGetIPEndPointTests()
        {
            var expectedEP = new IPEndPoint(IPAddress.Loopback, 20722);
            Assert.IsTrue(TryGetIPEndPoint(expectedEP.ToString(), out var parsedEP));
            Assert.AreEqual(expectedEP, parsedEP);

            Assert.IsFalse(TryGetIPEndPoint("8.8.8.256:2022", out var _));
        }
    }
}

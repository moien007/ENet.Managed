using ENet.Managed;
using ENet.Managed.Native;

using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ENetExtensionMethodTests
    {
        [Test]
        public unsafe void TestSetAndGetUserDataPeer()
        {
            var nativePeer = new NativeENetPeer();
            var peer = new ENetPeer(&nativePeer);
            var data = 1399;

            peer.SetUserData(data);
            Assert.IsTrue(peer.TryGetUserData<int>(out var data2));
            Assert.AreEqual(data, data2);
        }

        [Test]
        public unsafe void TestSetAndGetUserDataPacket()
        {
            var nativePeer = new NativeENetPacket();
            var packet = new ENetPacket(&nativePeer);
            var data = 1399;

            packet.SetUserData(data);
            Assert.IsTrue(packet.TryGetUserData<int>(out var data2));
            Assert.AreEqual(data, data2);
        }
    }
}

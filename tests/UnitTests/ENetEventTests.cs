using System;
using System.Collections.Generic;
using System.Text;

using ENet.Managed;
using ENet.Managed.Native;

using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class ENetEventTests
    {
        class Listener : IENetEventListener
        {
            public bool OnConnectCalled, OnDisconnectCalled, OnReceiveCalled;

            public void OnConnect(ENetPeer peer, uint data)
            {
                OnConnectCalled = true;
            }

            public void OnDisconnect(ENetPeer peer, uint data)
            {
                OnDisconnectCalled = true;
            }

            public void OnReceive(ENetPeer peer, ENetPacket packet, byte channelId)
            {
                OnReceiveCalled = true;
            }
        }

        [Test]
        public void DispatchToListenerTest()
        {
            var listener = new Listener();
            var ev = default(ENetEvent);

            void setEvent(ENetEventType type)
            {
                var native = new NativeENetEvent();
                native.Type = type;
                ev = new ENetEvent(native);
            }

            setEvent(ENetEventType.Connect);
            Assert.IsTrue(ev.DisptachTo(listener));
            Assert.IsTrue(listener.OnConnectCalled);

            setEvent(ENetEventType.Disconnect);
            Assert.IsTrue(ev.DisptachTo(listener));
            Assert.IsTrue(listener.OnDisconnectCalled);

            setEvent(ENetEventType.Receive);
            Assert.IsTrue(ev.DisptachTo(listener));
            Assert.IsTrue(listener.OnReceiveCalled);

            setEvent(ENetEventType.None);
            Assert.IsFalse(ev.DisptachTo(listener));
        }
    }
}

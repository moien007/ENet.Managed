using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ENet.Managed;
using System.Net;
using System.IO;

namespace ENetChatSample
{
    public partial class ChatForm : Form
    {
        private ENetHost m_Host;
        private ENetPeer m_Peer;

        public bool IsClient { get; }

        private ChatForm()
        {
            InitializeComponent();
        }

        public ChatForm(IPEndPoint endPoint, bool connect) : this()
        {
            ManagedENet.Startup();

            IsClient = connect;
            nameBox.Text = string.Format("Test{0}", new Random().Next(1, 10));
            if (IsClient) chatBox.Enabled = false;

            m_Host = IsClient ? new ENetHost(1, 1) : new ENetHost(endPoint, ENetHost.MaximumPeers, 1);
            m_Host.OnConnect += Host_OnConnect;
            if (IsClient) m_Host.Connect(endPoint, 1, 0);
            m_Host.StartServiceThread();
        }

        private void Host_OnConnect(object sender, ENetConnectEventArgs e)
        {
            if (IsClient)
            {
                m_Peer = e.Peer;
                chatBox.Enabled = true;
                WriteLog("Connected");
            }
            else
                WriteLog("Peer connected from {0}", e.Peer.RemoteEndPoint);

            e.Peer.OnReceive += Peer_OnReceive;
            e.Peer.OnDisconnect += Peer_OnDisconnect;
        }

        private void Peer_OnDisconnect(object sender, uint e)
        {
            var peer = sender as ENetPeer;
            var data = e;

            if (IsClient)
                WriteLog("Disconnected from host");
            else
                WriteLog("Peer disconnected from {0}", peer.RemoteEndPoint);
        }

        private void Peer_OnReceive(object sender, ENetPacket e)
        {
            using (var reader = new StreamReader(e.GetPayloadStream(false)))
            {
                var name = reader.ReadLine();
                var text = reader.ReadToEnd();

                if (IsClient)
                {
                    WriteLog("{0}: {1}", name, text);
                }
                else
                {
                    WriteLog("{0}: {1}", name, text);
                    m_Host.Broadcast(e.GetPayloadFinal(), 0, ENetPacketFlags.Reliable);
                }
            }
        }

        private void WriteLog(string format, params object[] args)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => WriteLog(format, args)));
                return;
            }

            logBox.AppendText(string.Format(format, args) + Environment.NewLine);
            logBox.SelectionStart = logBox.Text.Length;
            logBox.ScrollToCaret();
        }

        private void chatBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.SuppressKeyPress = true;
            if (string.IsNullOrWhiteSpace(chatBox.Text)) return;

            byte[] payload;

            using (var memory = new MemoryStream())
            using (var writer = new StreamWriter(memory))
            {
                writer.WriteLine(nameBox.Text);
                writer.Write(chatBox.Text);
                writer.Flush();

                payload = memory.ToArray();
            }

            if (IsClient)
            {
                m_Peer.Send(payload, 0, ENetPacketFlags.Reliable);
            }
            else
            {
                m_Host.Broadcast(payload, 0, ENetPacketFlags.Reliable);
                WriteLog("{0}: {1}", nameBox.Text, chatBox.Text);
            }

            chatBox.Clear();
        }
    }
}

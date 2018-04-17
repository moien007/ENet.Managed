using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using System.Net;
using ENet.Managed;

namespace ChatSample
{
    public partial class Form1 : Form
    {
        private ENetHost _Host;
        private ENetPeer _Peer;

        public Form1()
        {
            InitializeComponent();
            ManagedENet.Startup();
            MessageBox.Show(string.Format("ENet Version: {0}", ManagedENet.LinkedVersion));
        }

        private void chatBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrWhiteSpace(chatBox.Text) && _Peer != null)
            {
                var text = chatBox.Text;
                chatBox.Clear();
                _Peer.Send(Encoding.UTF8.GetBytes(text), 0, ENetPacketFlags.Reliable);
                logBox.Text += string.Format("[{0}] You: {1}\n", DateTime.Now.ToString("HH:mm:ss"), text);
                return;
            }
        }

        private void createBtn_Click(object sender, EventArgs e)
        {
            int port;
            while (true)
            {
                var input = Interaction.InputBox("Enter port number to listen on:");
                if (int.TryParse(input, out port)) break;
            }

            CreateHost(port);
            this.Text += " (SERVER)";
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            int port;
            while (true)
            {
                var input = Interaction.InputBox("Enter port number to connect to:");
                if (int.TryParse(input, out port)) break;
            }

            CreateHost(0);
            _Host.Connect(new IPEndPoint(IPAddress.Loopback, port), 1, 0);
            this.Text += " (CLIENT)";
        }

        private void CreateHost(int port)
        {
            _Host = new ENetHost(new IPEndPoint(IPAddress.Loopback, port), 1, 1);
            _Host.OnConnect += _Host_OnConnect;
            _Host.OnDisconnect += _Host_OnDisconnect;
            _Host.OnReceive += _Host_OnReceive;

            timer1.Start();

            _Host.StartServiceThread();
        }

        private void _Host_OnReceive(object sender, ENetReceiveEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<ENetReceiveEventArgs>(_Host_OnReceive), sender, e);
                return;
            }

            var text = Encoding.UTF8.GetString(e.Packet.GetPayloadFinal());
            logBox.Text += string.Format("[{0}] Friend: {1}\n", DateTime.Now.ToString("HH:mm:ss"), text);
        }

        private void _Host_OnDisconnect(object sender, ENetDisconnectEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<ENetDisconnectEventArgs>(_Host_OnDisconnect), sender, e);
                return;
            }

            logBox.Text += string.Format("[{0}][HOST] Disconnected\n", DateTime.Now.ToString("HH:mm:ss"));
        }

        private void _Host_OnConnect(object sender, ENetConnectEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<ENetConnectEventArgs>(_Host_OnConnect), sender, e);
                return;
            }

            _Peer = e.Peer;
            logBox.Text += string.Format("[{0}][HOST] Connected\n", DateTime.Now.ToString("HH:mm:ss"));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            statusBox.Text = string.Format("In: {0} | Out: {1}", 
                ENetUtils.FormatBytes(_Host.TotalReceivedData),
                ENetUtils.FormatBytes(_Host.TotalSentData));
        }
    }
}

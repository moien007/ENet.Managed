using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace ENetChatSample
{
    public partial class MainForm : Form
    {
        private IPEndPoint m_EndPoint;

        public MainForm()
        {
            InitializeComponent();
            m_EndPoint = new IPEndPoint(IPAddress.Loopback, 27015);
            addressBox.Text = m_EndPoint.ToString();
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            new ChatForm(m_EndPoint, true).ShowDialog();
            this.Close();
        }

        private void createBtn_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            new ChatForm(m_EndPoint, false).ShowDialog();
            this.Close();
        }

        private void addressBox_TextChanged(object sender, EventArgs e)
        {
            if (TryParseAddress(addressBox.Text, out m_EndPoint))
            {
                addressBox.BackColor = Color.MintCream;
                connectBtn.Enabled = true;
                createBtn.Enabled = true;
            }
            else
            {
                addressBox.BackColor = Color.LightYellow;
                connectBtn.Enabled = false;
                createBtn.Enabled = false;
            }
        }

        static bool TryParseAddress(string text, out IPEndPoint endPoint)
        {
            endPoint = null;
            if (string.IsNullOrWhiteSpace(text)) return false;

            var split = text.Split(':');
            if (split.Length != 2) return false;

            if (!IPAddress.TryParse(split[0], out IPAddress address)) return false;
            if (!ushort.TryParse(split[1], out ushort port)) return false;
            if (port == 0) return false;

            endPoint = new IPEndPoint(address, port);
            return true;
        }
    }
}

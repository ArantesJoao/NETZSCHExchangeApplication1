using System;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace NETZSCHExchange1
{
    public partial class Form1 : Form
    {
        private const string wsUrl = "ws://localhost:3001";
        private ClientWebSocket ws = new ClientWebSocket();

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await ws.ConnectAsync(new Uri(wsUrl), CancellationToken.None);
            ListenForMessages();
        }

        private async void ListenForMessages()
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            while (ws.State == WebSocketState.Open)
            {
                var received = await ws.ReceiveAsync(buffer, CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer.Array, 0, received.Count);
                var json = JObject.Parse(message);
                if (json["source"].ToString() == "react")
                {
                    lblOutput.Text = json["data"].ToString();  // Changed from txtOutput.Text
                }
            }
        }

        private async void txtInput_TextChanged(object sender, EventArgs e)
        {
            var inputValue = string.IsNullOrEmpty(txtInput.Text) ? " " : txtInput.Text;  // ensure to send empty space if !txtInput.Text
            var json = new JObject
            {
                ["source"] = "net",
                ["data"] = inputValue
            };
            var message = Encoding.UTF8.GetBytes(json.ToString());
            Console.WriteLine("Sending from .NET:", json.ToString());
            await ws.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}

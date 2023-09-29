using System;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace NETZSCHExchange1
{
    public partial class Form1 : Form
    {
        private const string wsUrl = "ws://localhost:3001";
        private ClientWebSocket ws = new ClientWebSocket();
        private bool isForcedChange = false;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await TryConnect();
        }

        /// <summary>
        /// Attempts to establish WebSocket connection
        /// and does the necessary error handling
        /// </summary>
        private async Task TryConnect()
        {
            try
            {
                ws = new ClientWebSocket();
                await ws.ConnectAsync(new Uri(wsUrl), CancellationToken.None);
                ResetUI();
                ListenForMessages();
            }
            catch (Exception ex)
            {
                HandleConnectionError(ex);
            }
        }

        /// <summary>
        /// Resets the UI components to their default status
        /// </summary>
        private void ResetUI()
        {
            isForcedChange = true;
            txtInput.Text = "";
            txtInput.ForeColor = System.Drawing.Color.Black;
            txtInput.Enabled = true;
            lblOutput.Text = "";
            lblOutput.ForeColor = System.Drawing.Color.Black;
            isForcedChange = false;
        }

        /// <summary>
        /// Continuously listens for incoming WebSocket messages
        /// </summary>
        private async void ListenForMessages()
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    var received = await ws.ReceiveAsync(buffer, CancellationToken.None);
                    var message = Encoding.UTF8.GetString(buffer.Array, 0, received.Count);
                    var json = JObject.Parse(message);
                    if (json["source"].ToString() == "react")
                    {
                        lblOutput.Text = json["data"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleConnectionError(ex);
            }
        }

        /// <summary>
        /// Sends the current text as a WebSocket message 
        /// unless the change was programatically forced
        /// </summary>
        private async void txtInput_TextChanged(object sender, EventArgs e)
        {
            if (isForcedChange)
                return;

            try
            {
                var inputValue = string.IsNullOrEmpty(txtInput.Text) ? " " : txtInput.Text; // ensure to send empty space if !txtInput.Text
                var json = new JObject
                {
                    ["source"] = "net",
                    ["data"] = inputValue
                };
                var message = Encoding.UTF8.GetBytes(json.ToString());
                Console.WriteLine("Sending from .NET:", json.ToString());
                await ws.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                HandleConnectionError(ex);
            }
        }

        /// <summary>
        /// Handles connections errors and attempts to re-establish it
        /// </summary>
        private async void HandleConnectionError(Exception ex)
        {
            string errorMessage = "Error while connecting to the ";
            if (ex is WebSocketException)
            {
                errorMessage += "WebSocket";
            }
            else
            {
                errorMessage += "server";
            }

            txtInput.Text = errorMessage;
            txtInput.ForeColor = System.Drawing.Color.Red;
            txtInput.Enabled = false;

            lblOutput.Text = errorMessage;
            lblOutput.ForeColor = System.Drawing.Color.Red;

            await TryConnect();
        }
    }
}

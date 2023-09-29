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

        private async Task TryConnect()
        {
            try
            {
                ws = new ClientWebSocket(); // reinitialize webSocket if needed
                await ws.ConnectAsync(new Uri(wsUrl), CancellationToken.None);
                ResetUI(); // reset UI after successful connection
                ListenForMessages();
            }
            catch (Exception ex)
            {
                HandleConnectionError(ex);
            }
        }

        private void ResetUI()
        {
            isForcedChange = true; // set flag before forcing the text change
            txtInput.Text = "";
            txtInput.ForeColor = System.Drawing.Color.Black; // reset text color
            txtInput.Enabled = true; // enable txtInput
            lblOutput.Text = ""; // reset output label
            lblOutput.ForeColor = System.Drawing.Color.Black; // reset text color
            isForcedChange = false; // reset flag after changing the text
        }

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
            txtInput.ForeColor = System.Drawing.Color.Red; // Set the text color to red
            txtInput.Enabled = false; // Disable the txtInput

            lblOutput.Text = errorMessage; // Assuming lblOutput is a Label control, if it's a TextBox, use txtOutput.Text
            lblOutput.ForeColor = System.Drawing.Color.Red; // Set the text color to red

            await TryConnect();
        }
    }
}

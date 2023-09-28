using System;
using System.Text;
using System.Net.Http;
using System.Windows.Forms;

namespace NETZSCHExchange1
{
    public partial class Form1 : Form
    {
        private const string apiUrl = "http://localhost:3001/api/data/react";
        private HttpClient client = new HttpClient();

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                var response = await client.GetStringAsync(apiUrl);
                txtOutput.Text = response;
            }
            catch
            {
                // Handle connection errors, perhaps show a message to the user.
            }
        }

        private async void updateTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var response = await client.GetStringAsync(apiUrl);
                if (response != txtOutput.Text)
                {
                    txtOutput.Text = response;
                }
            }
            catch
            {
                // Handle connection errors.
            }
        }

        private async void txtInput_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var content = new StringContent("{\"data\":\"" + txtInput.Text + "\"}", Encoding.UTF8, "application/json");
                await client.PostAsync("http://localhost:3001/api/data/net", content);
            }
            catch
            {
                // Handle connection errors.
            }
        }
    }
}

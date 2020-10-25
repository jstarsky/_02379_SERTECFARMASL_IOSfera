using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace _02379_SERTECFARMASL_IOSfera
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private AsyncTcpClientService _asyncTcpClientService = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(34, 36);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "connect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.connect);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(35, 65);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(169, 20);
            this.textBox1.TabIndex = 2;
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(34, 101);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(724, 303);
            this.listBox1.TabIndex = 3;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(129, 36);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "disconnect";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.disconnect);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(225, 65);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(533, 20);
            this.textBox2.TabIndex = 5;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(225, 36);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 6;
            this.button3.Text = "Send Data";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.sendData);

            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        private async void connect(object sender, EventArgs e)
        {
            Console.Write("Array Min and Avg service is now running");
            int PORT = 7070;
            string HOST = "localhost";
            try
            {
                if (this._asyncTcpClientService == null)
                {
                    string warehouse = "FarmaciaAlmacen_1";
                    string workstation = "puesto_1";
                    if (warehouse != "")
                    {
                        listBox1.Items.Add($"Sent request to host {HOST} port {PORT}");
                        _asyncTcpClientService = new AsyncTcpClientService(HOST, PORT, warehouse, workstation);
                        Task<string> authConnetionResponse = _asyncTcpClientService.connet();
                        listBox1.Items.Add("Sent request, waiting for response ...");
                        await authConnetionResponse;
                        listBox1.Items.Add($"Received response: {authConnetionResponse.Result}");
                        if (!_asyncTcpClientService._authTcpClient.connected)
                        {
                            _asyncTcpClientService.disconnect();
                            _asyncTcpClientService = null;
                            listBox1.Items.Add("Connection not establish");
                        }
                        else
                        {
                            textBox2.Text = $"{_asyncTcpClientService._authTcpClient.id_socket}";
                        }
                    }
                }
                else
                {
                    listBox1.Items.Add("Connection already open");
                }

            }
            catch (Exception ex)
            {
                listBox1.Items.Add(ex.Message);
            }
        }
        private void disconnect(object sender, EventArgs e)
        {
            if (this._asyncTcpClientService != null)
            {
                listBox1.Items.Add($"Connection close from:  {_asyncTcpClientService.disconnect()}");
                _asyncTcpClientService = null;
            }
        }

        private void sendData(object sender, EventArgs e)
        {
            if (this._asyncTcpClientService != null)
            {
                string _responseConnetionResponse = _asyncTcpClientService.SendData(textBox1.Text);
                listBox1.Items.Add($"Received response: {_asyncTcpClientService.client.Connected} data: {_responseConnetionResponse}");
            }
        }

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button3;
        #endregion

    }


}


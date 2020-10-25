using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace _02379_SERTECFARMASL_IOSfera
{
    class AsyncTcpClientService
    {
        private int PORT;
        private string HOST;
        private string _warehouse;
        private string _workstation;
        private EndPoint localEndPoint;
        private string localAddress;
        private string locaPort;
        public TcpClient client;
        private NetworkStream networkStream;
        private StreamWriter writer;
        private StreamReader reader;
        public AuthTcpClient _authTcpClient;
        public ReciveDataTcpClient _reciveDataTcpClient;


        public AsyncTcpClientService(string host, int port, string warehouse, string workstation)
        {
            HOST = host;
            PORT = port;
            _warehouse = warehouse;
            _workstation = workstation;
        }

        public AsyncTcpClientService(string warehouse, string workstation)
        {
            HOST = "localhost";
            PORT = 7070;
            _warehouse = warehouse;
            _workstation = workstation;
        }

        public async Task<string> connet()
        {

            try
            {
                Console.WriteLine($"CONNECTION START");

                if (this.client != null)
                {
                    Console.WriteLine($"CONNECTION ALREADY OPEN");
                }
                else
                {
                    IPAddress ipAddress = null;
                    IPHostEntry ipHostInfo = Dns.GetHostEntry(this.HOST);
                    for (int i = 0; i < ipHostInfo.AddressList.Length; ++i)
                    {
                        if (ipHostInfo.AddressList[i].AddressFamily ==
                        AddressFamily.InterNetwork)
                        {
                            ipAddress = ipHostInfo.AddressList[i];
                            break;
                        }
                    }
                    this.client = new TcpClient();
                    await this.client.ConnectAsync(ipAddress, PORT);
                    this.networkStream = this.client.GetStream();
                    this.writer = new StreamWriter(this.networkStream);
                    this.reader = new StreamReader(this.networkStream);
                    this.writer.AutoFlush = true;
                    var _connet_json = new
                    {
                        warehouse = this._warehouse,
                        workstation = this._workstation,
                    };
                    string __connet = JsonConvert.SerializeObject(_connet_json);
                    await this.writer.WriteLineAsync(__connet);
                    string _response = await this.reader.ReadLineAsync();
                    string response = JObject.Parse(_response).ToString();
                    this._authTcpClient = JsonConvert.DeserializeObject<AuthTcpClient>(response);
                    return response;
                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ClientConnection: ArgumentNullException: {0}", e);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"ClientConnection: SocketException: {ex}");
            }
            return "";
        }

        public string disconnect()
        {
            this.client.Close();
            return null;
        }

        public string SendData(string data)
        {
            try
            {

                var _data_json = new
                {
                    data = data,
                };
                string _data_string = JsonConvert.SerializeObject(_data_json);
                this.writer.Write(_data_string);

                byte[] bytesToRead = new byte[this.client.ReceiveBufferSize];
                int bytesRead = this.networkStream.Read(bytesToRead, 0, this.client.ReceiveBufferSize);
                string response = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                this._reciveDataTcpClient = JsonConvert.DeserializeObject<ReciveDataTcpClient>(response);
                return response;
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"ClientConnection: ArgumentNullException: {ex}");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"ClientConnection: SocketException: {ex}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ClientConnection: Exception: {ex}");
                return $"{ex}";
            }
            return "";
        }

    }
    public class AuthTcpClient
    {
        public string id_socket;
        public string pharmacy_name;
        public bool connected;
    }

    public class ReciveDataTcpClient
    {
        public bool connected;
    }

}

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
using System.Threading;

namespace _02379_SERTECFARMASL_IOSfera
{
    class AsyncTcpClientService
    {
        private int PORT;
        private string HOST;
        private string _warehouse;
        private string _workstation;
        //private EndPoint localEndPoint;
        //private string localAddress;
        //private string locaPort;
        private TcpClient client;
        private NetworkStream networkStream;
        private StreamWriter writer;
        private StreamReader reader;
        private AuthTcpClient _authTcpClient;
        private ReciveDataTcpClient _reciveDataTcpClient;
        //private Thread _eRecepieThread;
        private Encoding _encoding;
        private bool _ready;


        public NetworkStream Stream => this.networkStream;

        public TcpClient Client => this.client;

        public AuthTcpClient Auth => this._authTcpClient;

        public ReciveDataTcpClient ReciveData => this._reciveDataTcpClient;

        //public Thread ERecepieThread => this._eRecepieThread;

        public bool Connected
        {
            get
            {
                try
                {
                    return this.client.Connected;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }


        public bool Ready => this._ready;

        public AsyncTcpClientService(string host, int port, string warehouse, string workstation)
        {
            this.HOST = host;
            this.PORT = port;
            this._warehouse = warehouse;
            this._workstation = workstation;
            this._encoding = Encoding.GetEncoding(1252);
            //this._eRecepieThread = new Thread(this.ERecepie);
            this._ready = false;
        }

        ~AsyncTcpClientService()
        {
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
                    this._ready = true;
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

        public bool disconnect()
        {
            this._ready = false;
            //this._eRecepieThread.Abort();
            //this._eRecepieThread = null;
            this.client.Close();
            return this._ready;
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

        private static async Task<string> ERecepie(NetworkStream networkStream, Encoding encoding)
        {
            byte[] bytesRequestArray = Enumerable.Empty<byte>().ToArray();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                const int count = 2;
                int bytesRequestRead = 0;
                do
                {
                    byte[] buf = new byte[count];
                    try
                    {
                        bytesRequestRead = await networkStream.ReadAsync(buf, 0, count);
                        await memoryStream.WriteAsync(buf, 0, bytesRequestRead);
                        if (bytesRequestRead <= 1)
                        {
                            bytesRequestRead = 0;
                            buf = new byte[count];
                            break;
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message + exception.StackTrace);
                    }
                } while (networkStream.CanRead && bytesRequestRead > 0);

                Console.WriteLine(bytesRequestArray);
                bytesRequestArray = memoryStream.ToArray();
                return encoding.GetString(bytesRequestArray);
            }
        }

        public async Task ERecepie()
        {
            await Task.Run(async () =>
            {
                byte[] bytesRequestArray = Enumerable.Empty<byte>().ToArray();
                while (this.Ready)
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        const int count = 1000;
                        int bytesRequestRead = 0;
                        do
                        {
                            byte[] buf = new byte[count];
                            try
                            {
                                bytesRequestRead = await networkStream.ReadAsync(buf, 0, count);
                                await memoryStream.WriteAsync(buf, 0, bytesRequestRead);
                                if (bytesRequestRead <= 1)
                                {
                                    bytesRequestRead = 0;
                                    buf = new byte[count];
                                    break;
                                }
                                break;
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine(exception.Message + exception.StackTrace);
                            }
                        } while (networkStream.CanRead && bytesRequestRead > 0);

                        bytesRequestArray = memoryStream.ToArray();
                        Console.WriteLine(this._encoding.GetString(bytesRequestArray));
                    }
                }
            });
        }
    }

    public class AuthTcpClient
    {
        public string id_socket;
        //public string pharmacy_name;
        public bool connected;
    }

    public class ReciveDataTcpClient
    {
        public bool connected;
    }

}


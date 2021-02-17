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
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Threading;
using System.Runtime.InteropServices;

namespace _02379_SERTECFARMASL_IOSfera
{
    class AsyncTcpClientService
    {
        private IPAddress IP;
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
        private Encoding _encoding;
        private bool _ready;
        private string _eRecepie = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><env:Envelope xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><env:Header /><env:Body env:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"><m:WS_CCFC_IdentificacioUsuariV2Response xmlns:m=\"http://ccfcserver.farmacat.org/\"><WS_CCFC_IdentificacioUsuariReturn xmlns:n1=\"http://ccfcserver.farmacat.org/schema/\" xsi:type=\"n1:DTOIdentificacioUsuariResponse\"><p_TIPUS_USUARI xsi:type=\"xsd:string\">F</p_TIPUS_USUARI><p_NIF xsi:type=\"xsd:string\">99999995C</p_NIF><p_COD_UP_DIS xsi:type=\"xsd:string\">12821</p_COD_UP_DIS><items_P_INF_EXE soapenc:arrayType=\"n1:DTO_P_INF_EXE[1]\"><DTO_P_INF_EXE xsi:type=\"n1:DTO_P_INF_EXE\"><p_COD_EXE xsi:type=\"xsd:string\">DISCCFC000</p_COD_EXE><p_DES_COD xsi:type=\"xsd:string\">Operació realitzada correctament - MISSATGE IMPORTANT DEL SERVEI: consultil a la WEB de SIFARE, o des del seu PGOF</p_DES_COD><p_EST_EXE xsi:type=\"xsd:string\">0</p_EST_EXE></DTO_P_INF_EXE></items_P_INF_EXE><p_EST_EXE xsi:type=\"xsd:string\">0</p_EST_EXE><items_AVIS_CCFC xsi:nil=\"true\" /></WS_CCFC_IdentificacioUsuariReturn></m:WS_CCFC_IdentificacioUsuariV2Response></env:Body></env:Envelope>";

        public NetworkStream Stream => this.networkStream;

        public TcpClient Client => this.client;

        public AuthTcpClient Auth
        {
            get
            {
                try
                {
                    return this._authTcpClient;

                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public string id
        {
            get
            {
                try
                {
                    if (this._authTcpClient != null)
                    {
                        return this._authTcpClient.id_socket;
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public ReciveDataTcpClient ReciveData => this._reciveDataTcpClient;

        public bool Connected
        {
            get
            {
                try
                {
                    if (this.client != null)
                    {
                        return this.client.Connected;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool Ready => this._ready;

        public string Host => this.HOST;
        public int Port => this.PORT;

        public AsyncTcpClientService(string host, int port, string warehouse, string workstation)
        {
            this.HOST = host;
            this.PORT = port;
            this._warehouse = warehouse;
            this._workstation = workstation;
            this._encoding = Encoding.GetEncoding(1252);
            this._ready = false;
        }

        public AsyncTcpClientService(IPAddress ip, int port, string warehouse, string workstation)
        {
            this.IP = ip;
            this.HOST = null;
            this.PORT = port;
            this._warehouse = warehouse;
            this._workstation = workstation;
            this._encoding = Encoding.GetEncoding(1252);
            this._ready = false;
        }

        ~AsyncTcpClientService()
        {
        }

        public async Task<string> ConnectAsync()
        {

            try
            {
                Console.WriteLine($"CONNECTION ASYNC START");

                if (this.client != null)
                {
                    Console.WriteLine($"CONNECTION ALREADY OPEN");
                }
                else
                {
                    if (this.HOST != null)
                    {
                        this.IP = null;
                        IPHostEntry ipHostInfo = Dns.GetHostEntry(this.HOST);
                        for (int i = 0; i < ipHostInfo.AddressList.Length; ++i)
                        {
                            if (ipHostInfo.AddressList[i].AddressFamily ==
                            AddressFamily.InterNetwork)
                            {
                                this.IP = ipHostInfo.AddressList[i];
                                break;
                            }
                        }
                    }
                    this.client = new TcpClient();
                    await this.client.ConnectAsync(this.IP, this.PORT);
                    this.networkStream = this.client.GetStream();
                    this.writer = new StreamWriter(this.networkStream);
                    this.reader = new StreamReader(this.networkStream);
                    this.writer.AutoFlush = true;
                    var _request_json = new
                    {
                        warehouse = this._warehouse,
                        workstation = this._workstation,
                    };
                    string _request = JsonConvert.SerializeObject(_request_json);
                    await this.writer.WriteLineAsync(_request);
                    string _response = await this.reader.ReadLineAsync();
                    string response = JObject.Parse(_response).ToString();
                    this._authTcpClient = JsonConvert.DeserializeObject<AuthTcpClient>(response);
                    this._ready = true;
                    return response;
                }
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"ClientConnection: ArgumentNullException: {ex}");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"ClientConnection: SocketException: {ex}");
            }
            return "";
        }

        public bool Connect()
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
                    if (this.HOST != null)
                    {
                        this.IP = null;
                        IPHostEntry ipHostInfo = Dns.GetHostEntry(this.HOST);
                        for (int i = 0; i < ipHostInfo.AddressList.Length; ++i)
                        {
                            if (ipHostInfo.AddressList[i].AddressFamily ==
                            AddressFamily.InterNetwork)
                            {
                                this.IP = ipHostInfo.AddressList[i];
                                break;
                            }
                        }
                    }
                    this.client = new TcpClient();
                    this.client.Connect(this.IP, this.PORT);
                    this.networkStream = this.client.GetStream();
                    this.writer = new StreamWriter(this.networkStream);
                    this.reader = new StreamReader(this.networkStream);
                    this.writer.AutoFlush = true;

                    this._ready = true;
                    return this._ready;
                }
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"ClientConnection: ArgumentNullException: {ex}");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"ClientConnection: SocketException: {ex}");
            }
            this._ready = false;
            return this._ready;
        }

        public bool Disconnect()
        {
            this._ready = false;
            try
            {
                if (this.client != null)
                {
                    this.client.Close();
                    return this._ready;
                }
                else
                {
                    return this._ready;
                }
            }
            catch
            {
                return this._ready;
            }

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
            string _request;
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
                                bytesRequestRead = await this.networkStream.ReadAsync(buf, 0, count);
                                await memoryStream.WriteAsync(buf, 0, bytesRequestRead);
                                var _response_json = new
                                {
                                    warehouse = this._warehouse,
                                    workstation = this._workstation,
                                    eRecepie = this._eRecepie
                                };
                                await this.writer.WriteLineAsync(JsonConvert.SerializeObject(_response_json));
                                //[DllImport("CCFCAPI.dll")] static extern short CCFCIdentificacioUsuari(UInt32 beepType);
                                break;
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine($"exception: {exception.Message} {exception.StackTrace}");
                            }
                        } while (networkStream.CanRead && bytesRequestRead > 0);

                        bytesRequestArray = memoryStream.ToArray();
                        _request = this._encoding.GetString(bytesRequestArray);

                        if (_request != "" && bytesRequestRead == 0)
                        {
                            Console.WriteLine($"Debug: {_request}");
                            bytesRequestRead = 0;
                            _request = "";
                        }
                    }
                }
            });
        }
    }

    public class AuthTcpClient
    {
        public string id_socket;
        public bool connected;
    }

    public class ReciveDataTcpClient
    {
        public bool connected;
    }
}


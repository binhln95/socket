using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using Common;
namespace Server
{
    public partial class Server : Form
    {
        IPEndPoint IP;
        Socket server;
        List<Socket> clientList;
        public string User = "Server";
        public Server()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;

            Connect();
        }

        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Gui tin cho tat ca cac client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text;
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            var obj = new DataChat() { Message = message, User = User };
            Send(obj);
            AddMessage("Server: " + txtMessage.Text);
            txtMessage.Clear();
        }

        /// <summary>
        /// Connect Server
        /// </summary>
        void Connect()
        {
            IP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            clientList = new List<Socket>();

            server.Bind(IP);

            Thread listen = new Thread(() => {
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Socket client = server.Accept();
                        clientList.Add(client);

                        Thread receipt = new Thread(Receive);
                        receipt.IsBackground = true;
                        receipt.Start(client);
                    }
                } catch
                {
                    IP = new IPEndPoint(IPAddress.Any, 9999);
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                }
                
            });
            listen.IsBackground = true;
            listen.Start();
        }

        /// <summary>
        /// Close Connect
        /// </summary>
        void Close()
        {
            server.Close();
        }

        /// <summary>
        /// Send message server
        /// </summary>
        void Send(DataChat dataChat)
        {
            foreach (Socket client in clientList)
            {
                var obj = new DataChat() { Message = txtMessage.Text, User = User };
                client.Send(Serialize(obj));
            }
        }

        /// <summary>
        /// Receipt message server
        /// </summary>
        void Receive(object obj)
        {
            Socket client = obj as Socket;
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    client.Receive(data);

                    var message = DeserializeDataChat<DataChat>(data);
                    if (message.isGetUser)
                    {
                        List<string> lstUser = new List<string>();
                        foreach(Socket soc in clientList)
                        {
                            if (soc != client)
                            {
                                lstUser.Add(soc.RemoteEndPoint.ToString());
                            }
                        }
                        message.lstUser = lstUser;
                        client.Send(Serialize(message));
                    } else if(!string.IsNullOrEmpty(message.IpEndPoint))
                    {
                        foreach (Socket soc in clientList)
                        {
                            if (soc.RemoteEndPoint.ToString() == message.IpEndPoint)
                            {
                                soc.Send(Serialize(message));
                            }
                        }
                    } else
                    {
                        AddMessage(message.User + ": " + message.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                clientList.Remove(client);
                client.Close();
            }
        }

        /// <summary>
        /// Add message to ListView
        /// </summary>
        /// <param name="message"></param>
        void AddMessage(string message)
        {
            lsvMessage.Items.Add(new ListViewItem() { Text = message });
        }

        byte[] Serialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, obj);

            return stream.ToArray();
        }

        object Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();

            return formatter.Deserialize(stream);
        }

        T DeserializeDataChat<T>(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                IFormatter br = new BinaryFormatter();
                var res = (T)br.Deserialize(ms);
                return res;
            }
        }
    }
}

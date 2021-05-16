using Common;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public partial class Client : Form
    {
        IPEndPoint IP;
        Socket client;
        public Client()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;

            Connect();

            lblResIpClient.Text = client.LocalEndPoint.ToString();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text;
            string user = txtUser.Text;
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(user))
            {
                return;
            }
            string endpoint = cmbFriends.SelectedItem != null ? cmbFriends.SelectedItem.ToString() : "";
            var obj = new DataChat() { Message = message, User = user, IpEndPoint = endpoint};
            Send(obj);
            AddMessage(txtUser.Text + ": " + txtMessage.Text);
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text;
            string user = txtUser.Text;
            var obj = new DataChat() { Message = message, isGetUser = true };
            Send(obj);
        }

        /// <summary>
        /// Connect Server
        /// </summary>
        void Connect()
        {
            IP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            try
            {
                client.Connect(IP);
            } catch (Exception ex )
            {
                MessageBox.Show("Không thể kết nối tới server");
                return;
            }

            Thread listen = new Thread(Receive);
            listen.IsBackground = true;
            listen.Start();
        }

        /// <summary>
        /// Close Connect
        /// </summary>
        void Close()
        {
            client.Close();
        }

        /// <summary>
        /// Send message server
        /// </summary>
        void Send(DataChat dataChat)
        {
            if (dataChat != null)
            {
                byte[] objectSerialize = Serialize(dataChat);
                client.Send(objectSerialize);
            }
        }

        /// <summary>
        /// Receipt message server
        /// </summary>
        void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    client.Receive(data);

                    var message = DeserializeDataChat<DataChat>(data);
                    if (message.isGetUser)
                    {
                        FillDataToComboBox(message);
                    }
                    else
                    {
                        AddMessage(message.User + ": " + message.Message);
                    }
                }
            } catch(Exception ex)
            {
                Console.WriteLine(ex);
                Close();
            }
        }

        private void FillDataToComboBox(DataChat message)
        {
            if (message.lstUser != null && message.lstUser.Count > 0)
            {
                cmbFriends.Items.Clear();
                cmbFriends.Items.Add("");
                foreach (string s in message.lstUser)
                {
                    cmbFriends.Items.Add(s);
                }
            }
        }

        /// <summary>
        /// Add message to ListView
        /// </summary>
        /// <param name="message"></param>
        void AddMessage(string message)
        {
            lsvMessage.Items.Add(new ListViewItem() { Text = message });
            txtMessage.Clear();
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

        private T Deserialize<T>(byte[] param)
        {
            using (MemoryStream ms = new MemoryStream(param))
            {
                IFormatter br = new BinaryFormatter();
                return (T)br.Deserialize(ms);
            }
        }

        /// <summary>
        /// Đóng kết nối
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }
    }
}

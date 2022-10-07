using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Threading;
using System.Text;
using System.Windows.Input;
using System.Windows.Controls;

namespace Chat_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private Thread receiver;
        public MainWindow()
        {
            // wpf stuff
            InitializeComponent();
            // initialize but not start the receiver thread
            receiver = new Thread(RecvLoop);
            receiver.IsBackground = true;
            ipBox.Focus();
        }

        ~MainWindow()
        {
            if (server.Connected)
            {
                try
                {
                    if (receiver.IsAlive)
                        receiver.Abort();
                    server.Disconnect(false);
                }
                catch (Exception ex)
                {
                    return;
                }
            }
        }

        private void ConnectClickHandler(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void ConnectKeydownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Connect();
            }
        }

        private void Connect()
        {
            chatBox.Text = "";
            string[] host_port = ipBox.Text.Split(':');
            if (host_port.Length != 2)
            {
                chatBox.Text = "Format: <IP>:<Port>";
                return;
            }

            try
            {
                Connect(host_port[0], host_port[1]);
                msgBox.Focus();
            }
            catch (Exception ex)
            {
                chatBox.Text = ex.Message;
            }
        }

        private void Connect(string hostname, string port_in)
        {
            IPHostEntry he = Dns.GetHostEntry(hostname);
            int port = Int32.Parse(port_in);

            foreach (IPAddress ip in he.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                    continue;

                IPEndPoint ipe = new IPEndPoint(ip, port);
                server.Connect(ipe);

                if (server.Connected)
                {
                    // gui stuff
                    statusLabel.Content = "Connected";
                    statusLabel.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Colors.Green
                    );
                    ipBox.IsEnabled = false;
                    connectBtn.IsEnabled = false;
                    // start receiving msgs
                    receiver.Start();
                    return;
                }
            }
        }

        private void SendClickHandler(object sender, RoutedEventArgs e)
        {
            SendMsg();
        }

        private void SendKeydownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMsg();
            }
        }

        private void SendMsg()
        {
            try
            {
                server.Send(Encoding.ASCII.GetBytes(msgBox.Text.ToCharArray()));
                chatBox.Text += "You: " + msgBox.Text + "\n";
            }
            catch (Exception ex)
            {
                chatBox.Text += ex.Message + "\n";
                return;
            }
            msgBox.Text = "";
        }

        private void RecvLoop()
        {
            while (true)
            {
                if (server.Available > 0)
                {
                    RecvMsg();
                }
            }
        }
        private void RecvMsg()
        {
            try 
            {
                byte[] buf = new byte[server.Available];
                server.Receive(buf);
                this.Dispatcher.Invoke(() =>
                {
                    chatBox.Text += "Server: " + Encoding.ASCII.GetString(buf) + "\n";
                }
                );
            }
            catch (Exception e)
            {
                chatBox.Text += e.Message + "\n";
            }
        }

        private void ScrollChangedHandler(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0)
            {
                chatBoxScroll.ScrollToVerticalOffset(chatBoxScroll.ExtentHeight);
            }
        }
    }
}

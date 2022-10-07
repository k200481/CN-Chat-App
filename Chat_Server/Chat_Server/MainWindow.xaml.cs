using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Threading;
using System.Text;
using System.Windows.Input;
using System.Windows.Controls;

namespace Chat_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread listener;
        private Thread receiver;
        private Socket client;

        public MainWindow()
        {
            InitializeComponent();
            receiver = new Thread(RecvMsgs);
            receiver.IsBackground = true;
            portBox.Focus();
        }

        ~MainWindow()
        {
            if (client != null && client.Connected)
            {
                try
                {
                    if(receiver.IsAlive)
                        receiver.Abort();
                    if (listener.IsAlive)
                        listener.Abort();
                    client.Disconnect(false);
                }
                catch (Exception ex)
                {
                    return;
                }
            }
        }

        private void ListenClickHandler(object sender, RoutedEventArgs e)
        {
            Listen();
        }

        private void ListenKeyHandler(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Listen();
            }
        }

        private void Listen()
        {
            statusLabel.Content = "Listening";
            statusLabel.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Colors.Yellow
            );
            
            listener = new Thread(() => Listen(this.Dispatcher.Invoke(() => portBox.Text)));
            listener.IsBackground = true;
            listener.Start();

            portBox.IsEnabled = false;
            listenBtn.IsEnabled = false;
            InputBox.Focus();
        }

        private void Listen(string port_in)
        {
            try
            {
                int port = Int32.Parse(port_in);
                Socket skt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                skt.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
                skt.Listen(1);
                client = skt.Accept();
                receiver.Start();

                this.Dispatcher.Invoke(() =>
                {
                    statusLabel.Content = "Connected";
                    statusLabel.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Colors.Green
                    );
                });
            }
            catch (Exception e)
            {
                this.Dispatcher.Invoke(() => {
                    chatBox.Text += e.Message + "\n";
                    listenBtn.IsEnabled = true;
                    portBox.IsEnabled = true;
                    statusLabel.Content = "Not Connected";
                    statusLabel.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Colors.Red
                    );
                    portBox.Focus();
                });
            }
        }

        private void SendClickHandler(object sender, RoutedEventArgs e)
        {
            SendMsg();
        }

        private void SendKeyHandler(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                SendMsg();
            }
        }

        private void SendMsg()
        {
            try
            {
                if (client == null)
                    throw new Exception("Not connected");

                client.Send(Encoding.ASCII.GetBytes(InputBox.Text.ToCharArray()));
                chatBox.Text += "You: " + InputBox.Text + "\n";
            }
            catch (Exception ex)
            {
                chatBox.Text += ex.Message + "\n";
            }
            InputBox.Text = "";
        }

        private void RecvMsgs()
        {
            while (true)
            {
                if(client != null && client.Available > 0)
                {
                    byte[] buf = new byte[client.Available];
                    client.Receive(buf);
                    this.Dispatcher.Invoke(() =>
                    {
                        chatBox.Text += "Client: " + Encoding.ASCII.GetString(buf) + "\n";
                    });
                }
            }
        }

        private void ScrollChangedHandler(object sender, ScrollChangedEventArgs e)
        {
            if(e.ExtentHeightChange != 0)
            {
                chatBoxScroll.ScrollToVerticalOffset(chatBoxScroll.ExtentHeight);
            }
        }
    }
}

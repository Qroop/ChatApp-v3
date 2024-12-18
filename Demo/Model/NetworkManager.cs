using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;
using ChatApp.View;

namespace ChatApp.Model
{
    public class NetworkManager : INotifyPropertyChanged
    {
        private string username;
        private bool isServer;
        readonly int port;
        readonly IPAddress address;
        private ObservableCollection<string> observableCollection = new ObservableCollection<string>();
        public event EventHandler OnApproved;
        public event EventHandler<string> OnRejected;
        private NetworkStream stream;

        public event PropertyChangedEventHandler PropertyChanged;

        public NetworkManager(bool isServer, IPAddress address, int port, string username)
        {
            this.isServer = isServer;
            this.address = address;
            this.port = port;
            this.username = username;

            startConnection();
        }

        private void OnPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; OnPropertyChanged("Message"); }
        }

        public bool startConnection()
        {


            Task.Factory.StartNew(() =>
            {
                var ipEndPoint = new IPEndPoint(address, port);
                TcpListener server = new TcpListener(ipEndPoint);
                TcpClient endPoint = null;

                if(this.isServer)
                {
                    try
                    {
                        server.Start();
                        Debug.WriteLine("Start listening...");
                        endPoint = server.AcceptTcpClient();
                        Debug.WriteLine("Connection accepted!");
                        handleConnection(endPoint);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error starting server: " + ex.ToString());
                    }
                } else
                {
                    endPoint = new TcpClient();
                    try
                    {
                        Debug.WriteLine("Connecting to the server...");
                        endPoint.Connect(ipEndPoint);
                        Debug.WriteLine("Connection established. Waiting for approval.");
                        // Skicka användarnamn till servern
                        this.stream = endPoint.GetStream();
                        sendChar(this.username + "~?");
                        handleConnection(endPoint);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error connecting to server: " + ex.ToString());
                    }
                    finally
                    {
                        endPoint.Close();
                    }
                }
            });

            return true;
        }

        private void handleConnection(TcpClient endPoint)
        {
            stream = endPoint.GetStream();
            while (true)
            {
                var buffer = new byte[1024];
                int received = stream.Read(buffer, 0, 1024);

                if (received == 0) 
                { 
                    Debug.WriteLine("received == 0, closing connection.");
                    break; 
                }

                var message = Encoding.UTF8.GetString(buffer, 0, received);
                this.Message = message;

                Debug.WriteLine("Message received: " + message + " server: " + isServer);

                // regex \A\w+~\?\z
                Regex regex = new Regex(@"\A\w+~\?\z");
                
                if (regex.IsMatch(message))
                {
                    Debug.WriteLine("It's a match!");

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AccessPopup apa = new AccessPopup(this);
                        apa.Show();
                        apa.ButtonNo.Click += (object sender, RoutedEventArgs e) => { apa.Close(); };
                        apa.ButtonYes.Click += (object sender, RoutedEventArgs e) => { apa.Close(); };
                    });
                }
                else if (message == "APPROVED")
                {
                    OnApproved?.Invoke(this, EventArgs.Empty);
                }
                else if (message == "DENIED")
                {
                    OnRejected?.Invoke(this, "Client denied by server.");
                }
            }
        }

        public void sendChar(string str)
        {
            Debug.WriteLine("sendChar(" + str + ")");
            Task.Factory.StartNew(() =>
            {
                var buffer = Encoding.UTF8.GetBytes(str);
                stream.Write(buffer, 0, str.Length);
            });
        }

        public Task<bool> WaitForServerApproval(NetworkManager networkManager)
        {
            var tcs = new TaskCompletionSource<bool>();

            networkManager.OnApproved += (sender, args) =>
            {
                tcs.SetResult(true);
                Debug.WriteLine("Client approved");
            };

            networkManager.OnRejected += (sender, error) =>
            {
                tcs.SetResult(false);
                Debug.WriteLine("Client denied");
            };

            return tcs.Task;
        }
    }
}
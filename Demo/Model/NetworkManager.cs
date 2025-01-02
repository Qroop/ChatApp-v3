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
using System.IO;
using System.Text.Json;

namespace ChatApp.Model
{
    using History = Dictionary<string, List<Message>>;
    using Database = Dictionary<string, Dictionary<string, List<Message>>>;

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
        public string currentReceiver = "jeswa278";

        private Database database = new Database();
        private History conversations = new History();
        private List<Message> currentConversation = new List<Message>();

        public event PropertyChangedEventHandler PropertyChanged;

        bool historySent = false;

        public NetworkManager(bool isServer, IPAddress address, int port, string username)
        {
            this.isServer = isServer;
            this.address = address;
            this.port = port;
            this.username = username;

            if (isServer)
            {
                // Read from json
                this.database = ReadJson();
                //this.conversations = database[username];
            }



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

        private List<Message> messages = new List<Message>();
        public List<Message> Messages 
        { 
            get
            {
                return messages;
            }
            set
            {
                messages = value;
                OnPropertyChanged(nameof(this.Messages));
            }
        }


        public string Message
        {
            get { return message; }
            set { message = value; OnPropertyChanged("Message"); }
        }

        private Database ReadJson()
        {
            string FileName = @"..\..\Model\db.json";
            string content = File.ReadAllText(FileName);
            var db = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
            Debug.WriteLine(db);

            return null;
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
                        sendReq(this.username + "~?");
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

                if(isServer && !historySent)
                {
                    if(this.Messages.Count > 0)
                    {
                        sendHistory();
                    }
                    this.historySent = true;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, received);
                this.Message = message;

                //Debug.WriteLine("Message received: " + message + " server: " + isServer);

                Regex regex = new Regex(@"\A\w+~\?\z");
                
                if (regex.IsMatch(message))
                {
                    Debug.WriteLine("It's a match!");

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AccessPopup apa = new AccessPopup(this, message);
                        apa.Show();
                        apa.ButtonNo.Click += (object sender, RoutedEventArgs e) => { apa.Close(); };
                        apa.ButtonYes.Click += (object sender, RoutedEventArgs e) => { apa.Close(); };
                    });
                    continue;
                }
                else if (message == "APPROVED")
                {
                    OnApproved?.Invoke(this, EventArgs.Empty);
                    continue;
                }
                else if (message == "DENIED")
                {
                    OnRejected?.Invoke(this, "Client denied by server.");
                    continue;
                }

                AddMessage(message);
            }
        }

        private void AddMessage(string msg)
        {
            Message objMessage = new Message(msg);
            this.Messages.Add(objMessage);

            /*
            Om du är servern
            1. 
             */

            OnPropertyChanged(nameof(this.Messages));
        }

        private void sendHistory()
        {
            foreach(Message message in this.Messages)
            {
                string stringMessage = message.ToString();
                sendChar(stringMessage);
            }
        }

        public void sendChar(string str)
        {
            Message message;
            try
            {
                message = new Message(str);
            }
            catch (Exception e) { Debug.WriteLine(e); }
            finally
            {
                message = new Message(this.username, this.currentReceiver, str, this.isServer);
            }
            string stringMessage = message.ToString();
            //Debug.WriteLine("sendChar(" + stringMessage + ")");
            Task.Factory.StartNew(() =>
            {
                byte[] buffer = Encoding.UTF8.GetBytes(stringMessage);
                stream.Write(buffer, 0, stringMessage.Length);
            });
            this.Messages.Add(message);

            OnPropertyChanged(nameof(this.Messages));
        }

        public void sendReq(string str)
        {
            Task.Factory.StartNew(() =>
            {
                byte[] buffer = Encoding.UTF8.GetBytes(str);
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

        private void LoadHistory()
        {
            
        }
        
        private void SwtichConversation(string user)
        {
            // Om konversationen är tom?
            // Det kanske redan är hanterat
            this.currentReceiver = user;
            this.currentConversation = conversations[user];
        }
    }
}

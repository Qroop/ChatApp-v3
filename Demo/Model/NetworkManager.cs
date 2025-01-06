using ChatApp.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace ChatApp.Model
{
    using Database = Dictionary<string, Dictionary<string, List<Message>>>;
    using History = List<Tuple<string, List<Message>>>;

    public class NetworkManager : INotifyPropertyChanged
    {
        private readonly string username;
        private readonly bool isServer;
        private bool hasRecievedUsername = false;
        readonly int port;
        readonly IPAddress address;
        public event EventHandler OnApproved;
        public event EventHandler<string> OnRejected;
        private NetworkStream stream;
        public string receiver;
        private DateTime timeOfConnection;

        public bool? couldConnect;

        private Database database = new Database();

        private Dictionary<string, List<Message>> conversationsToView = new Dictionary<string, List<Message>>();

        private List<Message> toDisplay = new List<Message>();
        public List<Message> ToDisplay { get { return toDisplay; } set { toDisplay = value; } }

        private List<Tuple<string, DateTime>> conversations = new List<Tuple<string, DateTime>>();
        public List<Tuple<string, DateTime>> Conversations 
        {
            get => conversations;
            set => conversations = value; 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public NetworkManager(bool isServer, IPAddress address, int port, string username)
        {
            this.isServer = isServer;
            this.address = address;
            this.port = port;
            this.username = username;

            if (isServer)
            {
                this.database = ReadJson();
            }

            this.ToDisplay = this.Messages;

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
        private History history;
        private bool connected;

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
            // Läs in json filen till Database
            string FileName = @"..\..\Model\db.json";
            string content = File.ReadAllText(FileName);
            Database db = JsonSerializer.Deserialize<Database>(content);

            // Skapa en ny history
            History history = new History();

            // Om inte servern finns med i databasen, returnera en ny history bara
            if (!db.ContainsKey(this.username))
            {
                this.history = history;
                return db;
            }

            // conversationsToView innehåller timestamps kopplade till konversationer
            Dictionary<string, List<Message>> conversationsToView = db[this.username];

           this.conversationsToView = conversationsToView;

            foreach(string timestamp in conversationsToView.Keys)
            {
                string client = "";
                if (conversationsToView[timestamp].Count == 0) { continue; }
                if (conversationsToView[timestamp][0].SentFromServer) { client = conversationsToView[timestamp][0].Receiver; }
                else { client = conversationsToView[timestamp][0].Sender; }

                history.Add(new Tuple<string, List<Message>>(client, new List<Message>()));

                this.Conversations.Add(new Tuple<string, DateTime>(client, DateTime.Parse(timestamp)));

                foreach(Message message in conversationsToView[timestamp])
                {
                    history[history.Count - 1].Item2.Add(message);
                }
            }

            this.Conversations.Sort((x, y) => y.Item2.CompareTo(x.Item2));

            OnPropertyChanged(nameof(this.Conversations));
            return db;
        }

        private void startConnection()
        {
            Task.Factory.StartNew(() =>
            {
                var ipEndPoint = new IPEndPoint(address, port);
                TcpListener server = new TcpListener(ipEndPoint);
                TcpClient endPoint = null;

                if (this.isServer)
                {
                    try
                    {
                        server.Start();
                        Debug.WriteLine("Start listening...");
                        this.couldConnect = true;
                        endPoint = server.AcceptTcpClient();
                        Debug.WriteLine("Connection accepted!");
                        this.connected = true;
                        this.timeOfConnection = DateTime.Now;
                        handleConnection(endPoint);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error starting server: " + ex.ToString());
                        this.couldConnect = false;
                        this.connected = false;
                    }
                }
                else
                {
                    endPoint = new TcpClient();
                    try
                    {
                        Debug.WriteLine("Connecting to the server...");
                        endPoint.Connect(ipEndPoint);
                        this.couldConnect = true;
                        Debug.WriteLine("Connection established. Waiting for approval.");
                        this.stream = endPoint.GetStream();
                        this.connected = true;
                        handleConnection(endPoint);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error connecting to server: " + ex.ToString());
                        this.couldConnect = false;
                        this.connected = false;
                    }
                }
            });
        }

        private void handleConnection(TcpClient endPoint)
        {
            stream = endPoint.GetStream();
            while (true)
            {
                var buffer = new byte[1024];
                int received = stream.Read(buffer, 0, 1024);

                var message = Encoding.UTF8.GetString(buffer, 0, received);
                Message messageObj = Model.Message.FromJson(message);
                Debug.WriteLine($"Received {messageObj.Content}");

                if(received == 0)
                {
                    this.stream = null;
                    this.connected = false;
                }

                if (!isServer && !this.hasRecievedUsername)
                {
                    Debug.WriteLine("Received username: " + messageObj.Sender);
                    this.receiver = messageObj.Sender;
                    this.hasRecievedUsername = true;
                    OnPropertyChanged(nameof(this.receiver));
                }

                if(this.receiver == null && isServer)
                {
                    OnPropertyChanged();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AccessPopup accessPopup = new AccessPopup(this, messageObj.Sender);
                        accessPopup.Show();
                        accessPopup.ButtonNo.Click += (object sender, RoutedEventArgs e) => { accessPopup.Close(); };
                        accessPopup.ButtonYes.Click += (object sender, RoutedEventArgs e) => 
                        {
                            Debug.WriteLine("In Dispatcher.Invoke block");
                            accessPopup.Close();
                            this.receiver = messageObj.Sender;
                            Debug.WriteLine($"Receiver: {this.receiver}");
                            this.Conversations.Insert(0, new Tuple<string, DateTime>(this.receiver, this.timeOfConnection));
                            Debug.WriteLine(this.Conversations[0].Item1);
                            OnPropertyChanged(nameof(this.Conversations));
                            Debug.WriteLine("Conversation added");
                        };
                    });
                    continue;
                }
                else if(messageObj.Request == "ConnectionResponse")
                {
                    if (messageObj.Content == "APPROVED")
                    {
                        OnApproved?.Invoke(this, EventArgs.Empty);
                        OnPropertyChanged();
                        this.connected = true;
                        continue;
                    }
                    else if (messageObj.Content == "DENIED")
                    {
                        this.connected = false;
                        OnRejected?.Invoke(this, "Client denied by server.");
                        continue;
                    }
                }
                AddMessage(messageObj);
                OnPropertyChanged(nameof(this.Messages));
            }
        }

        private void AddMessage(Message message)
        {
            this.Messages.Add(message);
            OnPropertyChanged(nameof(this.Messages));
        }

        public void sendChar(string str)
        {
            Message message = new Message("Message", this.username, this.receiver, str, this.isServer);
            string stringMessage = message.ToString();
            sendMessage(message);
            
            this.Messages.Add(message);

            OnPropertyChanged(nameof(this.Messages));
        }


        public void sendReq(string str)
        {
            Debug.WriteLine("sendReq called");
            Message message = new Message("ConnectionResponse", this.username, this.receiver, str, true);
            sendMessage(message);
        }

        public void sendResp(string str)
        {
            Debug.WriteLine("sendResp called");
            Message message = new Message("ConnectionRequest", this.username, this.receiver, str, false);
            sendMessage(message);

        }

        private void sendMessage(Message message)
        {
            if(!connected) { return; }
            Debug.WriteLine("Sending message: " + message.Content + " as type: " + message.Request);
            Task.Factory.StartNew(() =>
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message.ToJson());
                this.stream.Write(buffer, 0, buffer.Length);
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

        public void SwitchConversation(DateTime dateTime)
        {
            Debug.WriteLine("Switching conversation to: " + dateTime.ToString());
            if(dateTime == this.timeOfConnection)
            {
                this.ToDisplay = this.Messages;
                OnPropertyChanged(nameof(ToDisplay));
                return;
            }
            this.ToDisplay = conversationsToView[dateTime.ToString()];
            OnPropertyChanged(nameof(ToDisplay));
        }

        public void Disconnect()
        {
            if(this.stream.CanRead && this.stream.CanWrite && this.stream != null && this.connected)
            {
                Debug.WriteLine($"Sending message from {this.isServer.ToString()}");
                Message closing = new Message("ConnectionStatus", this.username, this.receiver, $"{this.username} has disconnected", this.isServer);
                sendMessage(closing);
            }
            
            if(!isServer)
            {
                return;
            }

            this.receiver = "nobody";
            OnPropertyChanged(nameof(this.receiver));

            if (!this.database.ContainsKey(this.username)) 
            { 
                this.database.Add(this.username, new Dictionary<string, List<Message>>());
            }
            this.database[this.username].Add(this.timeOfConnection.ToString(), this.Messages);

            // Ifall sista meddelandet är en connectionstatus, ta bort det.
            if (this.database[this.username][this.timeOfConnection.ToString()][this.Messages.Count - 1].Request == "ConnectionStatus") 
            {
                this.database[this.username][this.timeOfConnection.ToString()].RemoveAt(Messages.Count - 1);
            }

            string jsonString = JsonSerializer.Serialize(this.database, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(@"..\..\Model\db.json", jsonString);

            this.stream.Close();
        }
    }
}

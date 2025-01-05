﻿using ChatApp.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ChatApp.Model
{
    using Database = Dictionary<string, Dictionary<string, List<Message>>>;
    using History = Dictionary<string, List<Message>>;

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
        public string currentReceiver;

        private Database database = new Database();
        private History conversations = new History();

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
            Dictionary<string, Dictionary<string, List<string>>> temp = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(content);

            Database db = new Database();
            foreach (string server in temp.Keys)
            {
                db.Add(server, new Dictionary<string, List<Model.Message>>());
                foreach (string client in temp[server].Keys)
                {
                    db[server].Add(client, new List<Model.Message>());
                    foreach (string message in temp[server][client])
                    {
                        db[server][client].Add(new Model.Message(message));
                    }
                }
            }

            if(db.ContainsKey(this.username))
            {
                this.conversations = db[this.username];
            }
            else
            {
                this.conversations = new History();
            }
            return db;
        }

        public bool startConnection()
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
                        endPoint = server.AcceptTcpClient();
                        Debug.WriteLine("Connection accepted!");
                        handleConnection(endPoint);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error starting server: " + ex.ToString());
                    }
                }
                else
                {
                    endPoint = new TcpClient();
                    try
                    {
                        Debug.WriteLine("Connecting to the server...");
                        endPoint.Connect(ipEndPoint);
                        Debug.WriteLine("Connection established. Waiting for approval.");
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

                var message = Encoding.UTF8.GetString(buffer, 0, received);
                Debug.WriteLine($"Received {message}");

                if (!isServer && this.hasRecievedUsername)
                {
                    this.currentReceiver = message;
                    this.hasRecievedUsername = false;
                    continue;
                }

                Regex username = new Regex(@"\A\w+~\?\z");
                if (username.IsMatch(message))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AccessPopup accessPopup = new AccessPopup(this, message);
                        accessPopup.Show();
                        accessPopup.ButtonNo.Click += (object sender, RoutedEventArgs e) => { accessPopup.Close(); };
                        accessPopup.ButtonYes.Click += (object sender, RoutedEventArgs e) => { accessPopup.Close(); };
                    });
                    this.currentReceiver = message.Substring(0, message.Length - 2);
                    continue;
                }
                else if (message == "APPROVED")
                {
                    OnApproved?.Invoke(this, EventArgs.Empty);
                    OnPropertyChanged();
                    this.hasRecievedUsername = true;
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
            OnPropertyChanged(nameof(this.Messages));
        }

        public void sendChar(string str)
        {
            Message message = new Message(this.username, this.currentReceiver, str, this.isServer);
            string stringMessage = message.ToString();
            sendMessage(stringMessage);
            
            this.Messages.Add(message);

            OnPropertyChanged(nameof(this.Messages));
        }

        public void sendReq(string str)
        {
            sendMessage(str);
            if (str == "APPROVED") 
            { AcceptClient(); }
        }

        private void sendMessage(string message)
        {
            Debug.WriteLine("Sending message: " + message);
            Task.Factory.StartNew(() =>
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                this.stream.Write(buffer, 0, buffer.Length);
            });
        }

        private void AcceptClient()
        {
            if(this.conversations.ContainsKey(this.currentReceiver)) 
            {
                this.Messages = this.conversations[this.currentReceiver];
            }
            else 
            { 
                this.Messages = new List<Message>(); 
            }
            OnPropertyChanged(nameof(this.Messages));

            sendMessage(this.username);

            foreach (Model.Message message in this.Messages)
            {
                sendMessage(message.ToString());
                Thread.Sleep(1);
            }
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

        public void SwtichUser(string clientUsername) 
        {
            this.conversations[this.currentReceiver] = this.Messages;
            this.currentReceiver = clientUsername;
            this.Messages = this.conversations[this.currentReceiver];
        }

        public void Disconnect()
        {
            this.stream.Close();
            if(!isServer)
            {
                return;
            }

            this.conversations[this.currentReceiver] = this.Messages;
            this.database[this.username] = this.conversations;
            var temp = new Dictionary<string, Dictionary<string, List<string>>>();

            foreach (string server in this.database.Keys)
            {
                temp.Add(server, new Dictionary<string, List<string>>());
                foreach (string client in this.database[server].Keys)
                {
                    temp[server].Add(client, new List<string>());
                    foreach (Message message in this.database[server][client])
                    {
                        temp[server][client].Add(message.ToString());
                    }
                }

            }
            string jsonString = JsonSerializer.Serialize(temp, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(@"..\..\Model\db.json", jsonString);
        }
    }
}

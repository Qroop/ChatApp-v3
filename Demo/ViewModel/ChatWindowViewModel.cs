using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using ChatApp.Model;
using ChatApp.View;
using ChatApp.ViewModel;
using ChatApp.ViewModel.Command;

namespace ChatApp.ViewModel
{
    internal class ChatWindowViewModel : INotifyPropertyChanged
    {
        private NetworkManager networkManager;
        public NetworkManager NetworkManager
        {
            get{ return networkManager; }
            set{ networkManager = value; }
        }

        private ICommand accept;
        private ICommand decline;
        private ICommand sendMessage;
        private string username = "placeholder";
        public string Username { get { return username; } set { username = value; } }

        private ObservableCollection<Message> messages = new ObservableCollection<Message>();
        public ObservableCollection<Message> Messages { get { return messages; } set { messages = value; OnPropertyChanged(nameof(Messages)); } }
        

        public ChatWindowViewModel(NetworkManager networkManager, string username = "~?") 
        {
            this.Username = username.Substring(0, username.Length - 2);
            this.networkManager = networkManager;
            Messages = new ObservableCollection<Message>(networkManager.Messages);
            this.networkManager.PropertyChanged += UpdateMessages;
        }

        private void UpdateMessages(object sender, PropertyChangedEventArgs e)
        {
            this.Messages = new ObservableCollection<Message>(this.networkManager.Messages);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string message;
        public string Message { 
            get
            { 
                return message; 
            } 
            set { 
                if (message != value)
                {
                    message = value;
                    OnPropertyChanged("Message");
                }
            }
        }
        private string chattingWith = "Chatting with ...";
        public string ChattingWith
        {
            get
            {
                return chattingWith;
            }
            set
            {
                this.chattingWith = $"Chatting with {value}";
                OnPropertyChanged("ChattingWith");
            }
        }

        public ICommand Accept
        {
            get 
            {
                if (accept == null)
                    accept = new AcceptCommand(this);
                return accept;
            }
            set 
            {
                accept = value;
            }
        }
        public ICommand Decline
        {
            get
            {
                if(decline == null)
                    decline = new DeclineCommand(this);
                return decline;
            }
            set
            {
                decline = value;
            }
        }

        public ICommand SendMessage
        {
            get
            {
                if (sendMessage == null)
                    sendMessage = new SendMessageCommand(this);
                return sendMessage;
            }
            set
            {
                sendMessage = value;
            }
        }
        public void AcceptConnection()
        {
            this.networkManager.sendReq("APPROVED");

        }


        public void DeclineConnection()
        {
            this.networkManager.sendReq("DENIED");
        }

        public void SendTheMessage()
        {
            //Debug.WriteLine(this.Message);
            this.NetworkManager.sendChar(this.Message);
            this.Message = "";
            OnPropertyChanged("Message");
        }

        public void CloseConnection()
        {
            this.NetworkManager.Disconnect();
        }
    }
}
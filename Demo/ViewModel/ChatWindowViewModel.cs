using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
        

        public ChatWindowViewModel(NetworkManager networkManager, string username = "~?") 
        {
            this.Username = username.Substring(0, username.Length - 2);
            NetworkManager = networkManager;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
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
            Debug.WriteLine(this.Message);
            this.networkManager.sendChar(this.Message);
            this.Message = "";
        }
    }
}
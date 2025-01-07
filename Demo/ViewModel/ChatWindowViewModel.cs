using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private string username;
        public string Username { get { return username; } set { username = value; } }

        private Tuple<string, DateTime> selectedUser = new Tuple<string, DateTime>("", new DateTime());
        public Tuple<string, DateTime> SelectedUser 
        { 
            get { return selectedUser; }
            set 
            { 
                selectedUser = value; 
                Debug.WriteLine(selectedUser.Item2);
                this.networkManager.SwitchConversation(selectedUser.Item2);
            } 
        }

        private ObservableCollection<Tuple<string, DateTime>> conversations = new ObservableCollection<Tuple<string, DateTime>>();
        public ObservableCollection<Tuple<string, DateTime>> Conversations { get { return conversations; } set { conversations = value; } }

        private ObservableCollection<Tuple<string, DateTime>> conversationsToSearch;
        private ObservableCollection<Message> messages = new ObservableCollection<Message>();
        public ObservableCollection<Message> Messages { get { return messages; } set { messages = value; OnPropertyChanged(nameof(Messages)); } }

        private string searchPhrase = "";
        public string SearchPhrase { get => searchPhrase; 
            set 
            { 
                searchPhrase = value; 
                OnPropertyChanged(nameof(this.SearchPhrase));
                FilterOnSearch();
                // networkManager.SearchConversations(searchPhrase);
                Debug.WriteLine(searchPhrase); 
            } 
        }
        

        public ChatWindowViewModel(NetworkManager networkManager, string username = "") 
        {
            this.Username = username;
            this.networkManager = networkManager;
            this.Messages = new ObservableCollection<Message>(this.networkManager.ToDisplay);
            this.Conversations = new ObservableCollection<Tuple<string, DateTime>>(this.networkManager.Conversations);
            this.conversationsToSearch = this.Conversations;
            this.networkManager.PropertyChanged += UpdateMessages;
        }

        private void UpdateMessages(object sender, PropertyChangedEventArgs e)
        {
            this.Messages = new ObservableCollection<Message>(this.networkManager.ToDisplay);
            this.Conversations = new ObservableCollection<Tuple<string, DateTime>>(this.networkManager.Conversations);
            this.ChattingWith = this.NetworkManager.receiver;
            OnPropertyChanged(nameof(this.Conversations));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void FilterOnSearch()
        {
            if (string.IsNullOrWhiteSpace(this.SearchPhrase))
            {
                this.Conversations = new ObservableCollection<Tuple<string, DateTime>>(this.networkManager.Conversations);
            }
            else
            {
                var filtered = this.networkManager.Conversations
                    .Where(session => session.Item1.IndexOf(this.SearchPhrase, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                this.Conversations = new ObservableCollection<Tuple<string, DateTime>>(filtered);
            }
            OnPropertyChanged(nameof(this.Conversations));
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
            if(this.Message == "") { return; }
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
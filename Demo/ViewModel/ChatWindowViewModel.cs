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
        private string username = "placeholder";
        public string Username { get { return username; } set { username = value; } }

        public ChatWindowViewModel(NetworkManager networkManager) 
        {
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

        public void AcceptConnection()
        {
            this.networkManager.sendChar("APPROVED");
        }


        public void DeclineConnection()
        {
            this.networkManager.sendChar("DENIED");
        }
    }
}
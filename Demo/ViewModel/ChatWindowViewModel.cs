﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ChatApp.Model;
using ChatApp.View;
using ChatApp.View.Command;

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
    }
}
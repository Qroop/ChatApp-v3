using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
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
    internal class MainWindowViewModel : INotifyPropertyChanged
    {

        // private NetworkManager NetworkManager { get; set; }
        private ICommand startServer;
        private ICommand startClient;

        private string ip = "127.0.0.1";
        private int port = 42069;
        private string username = "jeswa278";
        private string waitingText = "";
        public string WaitingText
        { 
            get 
            { 
                return waitingText; 
            } 
            set 
            { 
                waitingText = value; 
                Debug.WriteLine("Waiting text changed to: " + waitingText);
                OnPropertyChanged("WaitingText");
            } 
        }
        
        public string Ip { get { return ip; } set { ip = value; } }
        public int Port { get { return port; } set { port = value; } }
        public string Username { 
            get {

                return username; } 
            set 
            { 
                username = value;
                OnPropertyChanged("Username");
            } 
        }
        

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainWindowViewModel()
        {
        }

        private void myModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Message")
            {
                // var message = NetworkManager.Message;
                //this.MyText = message;
            }
        }

        public ICommand StartServer
        {
            get 
            {
                if(startServer == null)
                    startServer = new StartServerCommand(this); 
                return startServer;
            }
            set 
            {
                startServer = value;
            }
        }

        public ICommand StartClient
        {
            get
            {
                if (startClient == null)
                    startClient = new StartClientCommand(this);
                return startClient;
            }
            set
            {
                startClient = value;
            }
        }

        private NetworkManager EstablishConnection(bool isServer)
        {
            IPAddress address = IPAddress.Parse(Ip);
            return new NetworkManager(isServer, address, this.port, Username);
        }

        public void StartServerFunc()
        {
            ChatWindow cw = new ChatWindow(EstablishConnection(true));
            cw.Show();
            Application.Current.MainWindow.Close();
        }


        public async void StartClientFunc()
        {
            NetworkManager networkManager = EstablishConnection(false);
            Debug.WriteLine("Client awaiting response");
            networkManager.sendResp("I would like to join");
            this.WaitingText = "Waiting for approval...";
            bool status = false;
            
            try
            {
                status = await networkManager.WaitForServerApproval(networkManager);
            }
            catch
            {
                this.WaitingText = "Denied";
            }

            if (status)
            {
                ChatWindow cw = new ChatWindow(networkManager);
                cw.Show();
                Application.Current.MainWindow.Close();
            }
            else
            {
                this.WaitingText = "Denied";
            }
        }
    }
}

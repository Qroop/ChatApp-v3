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
        private string text;
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
        public string MyText { 
            get {

                return text; } 
            set 
            { 
                text = value;
                OnPropertyChanged("MyText");
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

        // private bool startConnection()
        // {
        //     return NetworkManager.startConnection();
        // }

        // public void startGameBoard()
        // {

        //     if (startConnection())
        //     {
        //         // GameBoard board = new GameBoard();
        //         //board.DataContext = this;
        //         //board.ShowDialog();
        //     }
        //     else
        //     {
        //         MessageBox.Show("Cannot start connection!");
        //     }
            
            
           
        // }

        private NetworkManager EstablishConnection(bool isServer)
        {
            IPAddress address = IPAddress.Parse(Ip);
            return new NetworkManager(isServer, address, this.Port);
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
            this.WaitingText = "Waiting for approval...";
            bool status = false;
            
            // 2. Wait for networkManager to give go ahead
            try
            {
                status = await networkManager.WaitForServerApproval(networkManager);
                // ChatWindow cw = new ChatWindow(networkManager);
                // cw.Show();
                // Application.Current.MainWindow.Close();
            }
            catch
            {
                this.WaitingText = "DENIED";
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




        // private ICommand enterCommand;
        // public ICommand EnterCommand
        // {
        //     get {
        //         if (enterCommand == null)
        //         {
        //             return new Command.KeyEnterCommand(this);
        //         }
        //         else {
        //             return enterCommand;
        //         }
        //     }
        //     set { enterCommand = value; }
        // }

        // public void sendMessage()
        // {
        //     NetworkManager.sendChar(MyText);
        // }

        //public void showGameBoard()
        //{
        //    GameBoard gameBoard = new GameBoard();
        //    gameBoard.DataContext = this;
        //    gameBoard.ShowDialog();
        //}


    }
}

using System;
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
    internal class MainWindowViewModel : INotifyPropertyChanged
    {

        private NetworkManager NetworkManager { get; set; }
        private ICommand startServer;
        private ICommand startClient;

        private string ip = "1.1.1.1";
        private string text;
        
        public string Ip { get { return ip; } set { ip = value; } }
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

        public MainWindowViewModel(NetworkManager networkManager)
        {
            NetworkManager = networkManager;

            networkManager.PropertyChanged += myModel_PropertyChanged;
        }

        private void myModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Message")
            {
                var message = NetworkManager.Message;
                this.MyText = message;
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
                    startClient = new StartServerCommand(this);
                return startClient;
            }
            set
            {
                startClient = value;
            }
        }

        private bool startConnection()
        {
            return NetworkManager.startConnection();
        }

        public void startGameBoard()
        {

            if (startConnection())
            {
                // GameBoard board = new GameBoard();
                //board.DataContext = this;
                //board.ShowDialog();
            }
            else
            {
                MessageBox.Show("Cannot start connection!");
            }
            
            
           
        }


        // NEW STUFF
        public void StartServerFunc()
        {

            ChatWindow cw = new ChatWindow();
            cw.Show();
            Application.Current.MainWindow.Close();
            // cw.CloseParent();
            // start server (ip, port, username)
            
        }

        public void StartClientFunc()
        {

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

        public void sendMessage()
        {
            NetworkManager.sendChar(MyText);
        }

        //public void showGameBoard()
        //{
        //    GameBoard gameBoard = new GameBoard();
        //    gameBoard.DataContext = this;
        //    gameBoard.ShowDialog();
        //}


    }
}

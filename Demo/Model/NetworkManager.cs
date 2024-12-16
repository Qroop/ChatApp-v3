using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatApp.Model
{
    internal class NetworkManager : INotifyPropertyChanged
    {
        bool isServer;
        int port;
        IPAddress address;

        private NetworkStream stream;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; OnPropertyChanged("Message"); }
        }

        public bool startConnection()
        {


            Task.Factory.StartNew(() =>
            {
                var ipEndPoint = new IPEndPoint(address, port);
                TcpListener server = new TcpListener(ipEndPoint);
                TcpClient endPoint = null;

                if(isServer)
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
                } else
                {
                    endPoint = new TcpClient();
                    try
                    {
                        Debug.WriteLine("Connecting to the server...");
                        endPoint.Connect(ipEndPoint);
                        Debug.WriteLine("Connection established!");
                        handleConnection(endPoint);
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
                var message = Encoding.UTF8.GetString(buffer, 0, received);
                this.Message = message;

            }

        }
        public void sendChar(string str)
        {
            Task.Factory.StartNew(() =>
            {
                var buffer = Encoding.UTF8.GetBytes(str);
                stream.Write(buffer, 0, str.Length);
            });
        }
    }
}

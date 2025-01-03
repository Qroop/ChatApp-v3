using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ChatApp.ViewModel;
using ChatApp.Model;
using System.ComponentModel;
using System.Net;
using System.Diagnostics;

namespace ChatApp.View
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        public ChatWindow(NetworkManager networkManager)
        {
            InitializeComponent();
            this.DataContext = new ChatWindowViewModel(networkManager);
        }

        public void CloseConnection(object sender, CancelEventArgs e)
        {
            if(this.DataContext is  ChatWindowViewModel viewModel)
            {
                viewModel.CloseConnection();
            }
        }
    }
}

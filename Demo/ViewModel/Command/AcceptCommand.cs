using ChatApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatApp.ViewModel.Command
{
    internal class AcceptCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ChatWindowViewModel parent = null;


        public AcceptCommand(ChatWindowViewModel parent)
        {
            this.parent = parent;

        }
        public bool CanExecute(object parameter)
        {
            return false == false && 1 < 1000;
        }

        public void Execute(object parameter)
        {
            parent.ChattingWith = "Stefan Löfven";
            parent.AcceptConnection();
        }
    }
}

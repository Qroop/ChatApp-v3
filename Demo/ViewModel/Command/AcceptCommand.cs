using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatApp.ViewModel;

namespace ChatApp.ViewModel.Command
{
    internal class AcceptCommand: ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ChatWidowViewModel parent = null;


        public AcceptCommand(ChatWindowViewModel parent)
        {
            this.parent = parent;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            parent.AcceptConnection();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatApp.ViewModel;

namespace ChatApp.View.Command
{
    internal class DeclineCommand: ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ChatWindowViewModel parent = null;


        public DeclineCommand(ChatWindowViewModel parent)
        {
            this.parent = parent;

        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            parent.DeclineConnection();
        }
    }
}

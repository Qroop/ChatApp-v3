using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatApp.ViewModel;

namespace ChatApp.View.Command
{
    internal class StartGameCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private MainWindowViewModel parent = null;


        public StartGameCommand(MainWindowViewModel parent)
        {
            this.parent = parent;

        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {

           // parent.startGameBoard();
        }
    }
}

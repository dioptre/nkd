using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NKD.Import.Client.UICommands
{
    public class DelegateCommand : ICommand
    {
        public delegate void ExecuteDelegate();
        public delegate bool CanExecuteDelegate();

        ExecuteDelegate _execute;
        CanExecuteDelegate _canExecute;

        public DelegateCommand(ExecuteDelegate execute, CanExecuteDelegate canExecute)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        bool ICommand.CanExecute(object parameter)
        {
            return _canExecute();
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        void ICommand.Execute(object parameter)
        {
            _execute();
        }
    }
}

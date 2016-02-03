using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NKD.Import.Client.UICommands
{
    public class MyCommand : ICommand
    {
        public void Execute(object parameter)
        {
            string hello = parameter as string;
            MessageBox.Show(hello, "World");
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }

    

}

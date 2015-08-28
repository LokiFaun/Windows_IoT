using System;
using System.Windows.Input;

namespace RoomMonitor
{
    internal class Command<T> : ICommand where T : class
    {
        private Action<T> m_Action = null;
        private Predicate<T> m_CanExecute = null;

        public Command(Action<T> action, Predicate<T> canExecute = null)
        {
            m_Action = action;
            m_CanExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if ((m_CanExecute == null) || !(parameter is T))
            {
                return true;
            }
            return m_CanExecute(parameter as T);
        }

        public void Execute(object parameter)
        {
            if (m_Action == null)
            {
                return;
            }
            if (parameter == null)
            {
                m_Action(null);
            }
            if (parameter is T)
            {
                m_Action(parameter as T); 
            }
        }
    }
}
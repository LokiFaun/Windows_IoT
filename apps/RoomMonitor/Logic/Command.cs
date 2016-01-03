namespace Dashboard.Logic
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// Command which can be executed from UI elements
    /// </summary>
    /// <typeparam name="T">Type of the parameter which can be used for command executing</typeparam>
    internal class Command<T> : ICommand where T : class
    {
        /// <summary>
        /// The action to execute
        /// </summary>
        private Action<T> m_Action = null;

        /// <summary>
        /// The predicate to check if the command can be executed
        /// </summary>
        private Predicate<T> m_CanExecute = null;

        /// <summary>
        /// Initializes a new instance of <see cref="Command{T}"/>
        /// </summary>
        /// <param name="action">The action to be executed by the command</param>
        /// <param name="canExecute">A method to check if the command be executed</param>
        public Command(Action<T> action, Predicate<T> canExecute = null)
        {
            m_Action = action;
            m_CanExecute = canExecute;
        }

        /// <summary>
        /// Event executed in case the command is able/unable to execute
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Executes the <see cref="CanExecuteChanged"/> event
        /// </summary>
        protected void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Checks if the command can be executed
        /// </summary>
        /// <param name="parameter">Parameter for checking</param>
        /// <returns><c>true</c> in case the command can be executed, else <c>false</c></returns>
        public bool CanExecute(object parameter)
        {
            if ((m_CanExecute == null) || !(parameter is T))
            {
                RaiseCanExecuteChanged();
                return true;
            }

            var canExecute = m_CanExecute(parameter as T);
            RaiseCanExecuteChanged();
            return canExecute;
        }

        /// <summary>
        /// Executes the action using the given parameter
        /// </summary>
        /// <param name="parameter">Parameter to execute the command with</param>
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
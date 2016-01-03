namespace Dashboard.ViewModel
{
    using System;

    using Logic;
    using Logic.Tasks;

    internal class MainViewModel : ViewModel
    {
        /// <summary>
        /// The IoC container
        /// </summary>
        private readonly Container m_Container;

        /// <summary>
        /// The current date/time
        /// </summary>
        private DateTime m_CurrentDateTime;

        /// <summary>
        /// The date/time update task
        /// </summary>
        private DateTimeTask m_DateTimeTask;

        /// <summary>
        /// The name of the MainViewModel.
        /// </summary>
        public const string MainViewModelName = "Main";

        /// <summary>
        /// Initializes an instance of <see cref="MainViewModel"/>
        /// </summary>
        /// <param name="container"></param>
        public MainViewModel(Container container)
        {
            m_Container = container;
            m_DateTimeTask = new DateTimeTask(container);
            m_DateTimeTask.Start();

            CurrentDateTime = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the current date/time
        /// </summary>
        public DateTime CurrentDateTime
        {
            get { return m_CurrentDateTime; }
            set
            {
                m_CurrentDateTime = value;
                NotifyPropertyChanged(nameof(CurrentDateTime));
            }
        }
    }
}

namespace Dashboard.Logic
{
    using System;
    using System.Threading;

    using ViewModel;

    /// <summary>
    /// Task for updating the displayed date and time in the main view-model.
    /// </summary>
    internal class DateTimeProvider : IDisposable
    {
        /// <summary>
        /// The update period for disabling the timer
        /// </summary>
        private const int Infinite = -1;

        /// <summary>
        /// The update period in seconds
        /// </summary>
        private const int Period = 1;

        /// <summary>
        /// The timer
        /// </summary>
        private readonly Timer m_Timer;

        /// <summary>
        /// Indicates whether the instance is disposed or not
        /// </summary>
        private bool m_IsDisposed = false;

        /// <summary>
        /// Initializes an instance of the <see cref="DateTimeProvider"/>
        /// </summary>
        /// <param name="container"></param>
        public DateTimeProvider(Container container)
        {
            m_Timer = new Timer(Callback, container, Infinite, Infinite);
        }

        /// <summary>
        /// Disposes the instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts the update task
        /// </summary>
        public void Start()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DateTimeProvider));
            }

            m_Timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(Period));
        }

        /// <summary>
        /// Stops the update task
        /// </summary>
        public void Stop()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DateTimeProvider));
            }

            m_Timer.Change(Infinite, Infinite);
        }

        /// <summary>
        /// Disposes the instance references
        /// </summary>
        /// <param name="disposing"><c>true</c> to dispose references</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                m_Timer.Dispose();
            }
            m_IsDisposed = true;
        }

        /// <summary>
        /// The timer callback for updating the main view-model
        /// </summary>
        /// <param name="state">The IoC container for resolving needed objects</param>
        private void Callback(object state)
        {
            var container = state as Container;
            if (container == null)
            {
                return;
            }

            var viewModel = container.ResolveNamed<MainViewModel>(MainViewModel.Name);
            if (viewModel == null)
            {
                return;
            }

            DispatcherHelper.RunOnUIThread(() =>
            {
                viewModel.CurrentDateTime = DateTime.Now;
            });
        }
    }
}
namespace Dashboard.ViewModel
{
    using Logic;
    
    /// <summary>
    /// The view-model locator.
    /// </summary>
    internal class ViewModelLocator
    {
        /// <summary>
        /// The global IoC container.
        /// </summary>
        private static Container m_Container = new Container();

        /// <summary>
        /// The DesignMode indicator.
        /// </summary>
        private static bool? m_InDesignMode = null;

        /// <summary>
        /// Gets the Main view-model.
        /// </summary>
        public static ViewModel Main
        {
            get
            {
                var viewModel = m_Container.ResolveNamed<MainViewModel>(MainViewModel.MainViewModelName);
                if (viewModel == null)
                {
                    viewModel = new MainViewModel(m_Container);
                    m_Container.RegisterNamed(MainViewModel.MainViewModelName, viewModel);
                }
                return viewModel;
            }
        }

        /// <summary>
        /// Gets whether the application is in design-mode or not.
        /// </summary>
        public static bool InDesignMode
        {
            get
            {
                if (!m_InDesignMode.HasValue)
                {
                    m_InDesignMode = Windows.ApplicationModel.DesignMode.DesignModeEnabled;
                }
                return m_InDesignMode.Value;
            }
        }
    }
}

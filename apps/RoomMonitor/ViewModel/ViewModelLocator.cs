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
        public Container Container = new Container();

        /// <summary>
        /// The DesignMode indicator.
        /// </summary>
        private bool? m_InDesignMode = null;

        /// <summary>
        /// Gets the Main view-model.
        /// </summary>
        public ViewModel Main
        {
            get
            {
                var viewModel = Container.ResolveNamed<MainViewModel>(MainViewModel.Name);
                if (viewModel == null)
                {
                    viewModel = new MainViewModel(Container);
                    Container.RegisterNamed(MainViewModel.Name, viewModel);
                }
                return viewModel;
            }
        }

        /// <summary>
        /// Gets whether the application is in design-mode or not.
        /// </summary>
        public bool InDesignMode
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

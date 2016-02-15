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
        /// Gets the News view-model.
        /// </summary>
        public ViewModel News
        {
            get
            {
                var viewModel = Container.ResolveNamed<NewsViewModel>(NewsViewModel.Name);
                if (viewModel == null)
                {
                    viewModel = new NewsViewModel(Container);
                    Container.RegisterNamed(NewsViewModel.Name, viewModel);
                }
                return viewModel;
            }
        }

        /// <summary>
        /// Gets the Sensors view-model.
        /// </summary>
        public ViewModel Sensors
        {
            get
            {
                var viewModel = Container.ResolveNamed<SensorsViewModel>(SensorsViewModel.Name);
                if (viewModel == null)
                {
                    viewModel = new SensorsViewModel(Container);
                    Container.RegisterNamed(SensorsViewModel.Name, viewModel);
                }
                return viewModel;
            }
        }
    }
}
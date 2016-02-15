// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Dashboard.View
{
    using ViewModel;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SensorPage : Page
    {
        public SensorPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var viewModel = DataContext as SensorsViewModel;
            if (viewModel != null)
            {
                viewModel.Update();
            }
        }
    }
}
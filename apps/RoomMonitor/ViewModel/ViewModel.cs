namespace Dashboard.ViewModel
{
    using System.ComponentModel;

    /// <summary>
    /// Base view-model.
    /// </summary>
    internal abstract class ViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Invoked when a property of the view-model changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invokes <see cref="PropertyChanged"/> event for the property with the name: <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyName">The name of the property to invoke the <see cref="PropertyChanged"/> event for.</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

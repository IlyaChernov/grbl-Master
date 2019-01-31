namespace grbl.Master.ViewModel
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBasee : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void OnPropertyChangedAuto([CallerMemberName]string propertyName = null)
        {
            OnPropertyChanged(propertyName);
        }
    }
}

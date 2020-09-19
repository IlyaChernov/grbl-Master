namespace grbl.Master.Model
{
    using System.Collections.ObjectModel;

    public class ApplicationSettings : NotifyPropertyChanged
    {
        public ObservableCollection<double> JoggingDistances { get; set; } = new ObservableCollection<double> { 0.01d, 0.1d, 1d, 5d, 10d, 100d };
        public double JoggingDistance { get; set; }

        public ObservableCollection<double> FeedRates { get; set; } = new ObservableCollection<double> { 5, 10, 50, 100, 500, 1000 };
        public double SliderLinearity { get; set; }

        public double FeedRate { get; set; }

        public ObservableCollection<Macros> Macroses { get; set; } = new ObservableCollection<Macros>();

        public int SelectedBaudRate { get; set; }
        
        public string SelectedComPort { get; set; }
    }
}

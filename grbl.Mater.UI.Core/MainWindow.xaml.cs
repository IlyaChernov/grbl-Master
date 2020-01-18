using System.Windows;

namespace grbl.Mater.UI.Core
{
    using System.Windows.Controls;

    using grbl.Mater.UI.Core.Services;

    using Microsoft.Extensions.Options;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ISampleService sampleService;
        private readonly AppSettings settings;

        public MainWindow(ISampleService sampleService, IOptions<AppSettings> settings)
        {
            InitializeComponent();
            this.sampleService = sampleService;
            this.settings = settings.Value;
        }

        private void NavigationDrawer_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
    }
}

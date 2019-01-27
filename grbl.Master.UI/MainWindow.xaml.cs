using grbl.Master.Communication;
using System.Windows;

namespace grbl.Master.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            COMService cs = new COMService();
            var ports = cs.GetPortNames();

            cs.DataReceived += Cs_DataReceived;

            cs.Connect("COM6", 115200);
        }

        private void Cs_DataReceived(object sender, string e)
        {
            
        }
    }
}

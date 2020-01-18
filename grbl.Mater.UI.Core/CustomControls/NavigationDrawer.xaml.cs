using System.Windows;
using System.Windows.Controls;

namespace grbl.Mater.UI.Core.CustomControls
{
    using System;

    /// <summary>
    /// Interaction logic for NavigationDrawer.xaml
    /// </summary>
    public partial class NavigationDrawer : UserControl
    {
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
        public NavigationDrawer()
        {
            InitializeComponent();
        }

        private void ButtonOpenMenuClick(object sender, RoutedEventArgs e)
        {
            this.ButtonCloseMenu.Visibility = Visibility.Visible;
            this.ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }

        private void ButtonCloseMenuClick(object sender, RoutedEventArgs e)
        {
            this.ButtonCloseMenu.Visibility = Visibility.Collapsed;
            this.ButtonOpenMenu.Visibility = Visibility.Visible;
        }

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(sender, e);
        }
    }
}

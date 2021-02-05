using System.Windows;
using System.Windows.Controls;

namespace grbl.Master.UI.Controls
{
    using JetBrains.Annotations;
    using System.Collections;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Data;

    /// <summary>
    /// Interaction logic for AxisBitMaskEditor.xaml
    /// </summary>
    public partial class AxisBitMaskEditor : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty DependencyProperty = DependencyProperty.Register(
            nameof(MaskValue),
            typeof(int),
            typeof(AxisBitMaskEditor),
            new PropertyMetadata(0, ChangeValue));

        private static void ChangeValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BitArray b = new BitArray(new[] { (int)e.NewValue });
            bool[] values = new bool[b.Length];
            b.CopyTo(values, 0);

            values = values.Take(6).ToArray();

            ((AxisBitMaskEditor)d).ValueBits = values;
        }

        public int MaskValue
        {
            get => (int)GetValue(DependencyProperty);
            set
            {
                SetValue(DependencyProperty, value);
                OnPropertyChanged(nameof(ValueBits));
                OnPropertyChanged();
            }
        }

        public bool[] ValueBits { get; set; } = { false, false, false, false, false, false };

        public AxisBitMaskEditor()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

       private void OnValueBitUpdated(object sender, DataTransferEventArgs e)
        {
            BitArray b = new BitArray(ValueBits.ToArray());
            int[] array = new int[1];
            b.CopyTo(array, 0);
            MaskValue = array[0];
        }
    }
}

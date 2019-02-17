namespace grbl.Master.Model
{
    using grbl.Master.Service.Annotations;
    using grbl.Master.Service.Enum;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class GrblStatus : INotifyPropertyChanged
    {
        private MachineState _machineState;

        public MachineState MachineState
        {
            get => _machineState;
            set
            {
                _machineState = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

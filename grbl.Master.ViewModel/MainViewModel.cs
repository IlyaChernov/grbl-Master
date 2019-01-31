using grbl.Master.Communication;
using System.Collections.ObjectModel;

namespace grbl.Master.ViewModel
{
    public class MainViewModel
    {
        private ObservableCollection<string> _comPorts;

        private readonly ICOMService _comService;

        public MainViewModel(ICOMService comService)
        {
            _comService = comService;
        }

        public ObservableCollection<string> ComPorts
        {
            get
            {
                return _comPorts;
            }
            set
            {
                _comPorts = value;
            }
        }
    }
}

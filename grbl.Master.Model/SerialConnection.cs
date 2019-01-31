using grbl.Master.Communication;
using System.Collections.Generic;

namespace grbl.Master.Model
{
    public class SerialConnection
    {

        public void ReloadComPorts()
        {
            COMService cs = new COMService();
            ComPorts = cs.GetPortNames();
        }

        public List<string> ComPorts
        {
            get;
            set;
        }
    }
}

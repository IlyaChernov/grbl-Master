namespace grbl.Master.BL
{
    using System;

    using grbl.Master.Common.Enum;
    using grbl.Master.Common.Interfaces.BL;
    using grbl.Master.Common.Interfaces.Service;
    using grbl.Master.Model.Enum;

    public class GrblDispatcher : IGrblDispatcher
    {
        private readonly IGrblStatus _grblStatus;

        public GrblDispatcher(IComService comService, IGrblPrompt grblPrompt, IGrblStatus grblStatus)
        {
            _grblStatus = grblStatus;
            grblPrompt.PromptReceived += GrblPromptPromptReceived;
            comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
        }

        private void ComServiceConnectionStateChanged(object sender, ConnectionState e)
        {
            if (e == ConnectionState.Offline)
            {
                _grblStatus.StopRequesting();
                _grblStatus.GrblStatusModel.MachineState = MachineState.Offline;
            }
            else
            {
                _grblStatus.GrblStatusModel.MachineState = MachineState.Online;
            }
        }

        private void GrblPromptPromptReceived(object sender, string e)
        {
            _grblStatus.InitialRequest();
            _grblStatus.StartRequesting(TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
        }
    }
}

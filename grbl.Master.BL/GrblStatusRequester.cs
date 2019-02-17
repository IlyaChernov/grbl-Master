namespace grbl.Master.BL
{
    using grbl.Master.BL.Interface;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Reactive;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;

    public class GrblStatusRequester : IGrblStatusRequester
    {
        private readonly ICommandSender _commandSender;
        readonly Subject<Unit> _stopSubject = new Subject<Unit>();
        private TimeSpan _interval = TimeSpan.Zero;

        public GrblStatusRequester(ICommandSender commandSender)
        {
            _commandSender = commandSender;
            commandSender.CommandFinished += SystemCommandSenderCommandFinished;
        }

        public bool IsRunning
        {
            get;
            set;
        }

        private void Request()
        {
            _commandSender.Send("?", CommandType.StatusRequest);
        }

        private async void SystemCommandSenderCommandFinished(object sender, Service.DataTypes.Command e)
        {
            if (IsRunning && e.Type == CommandType.StatusRequest)
            {
                await Task.Delay(_interval);
                Request();
            }
        }

        public void StartRequesting(TimeSpan interval)
        {
            _interval = interval;
            IsRunning = true;
            Request();
        }

        public void StopRequesting()
        {
            IsRunning = false;
            _stopSubject.OnNext(Unit.Default);
        }
    }
}

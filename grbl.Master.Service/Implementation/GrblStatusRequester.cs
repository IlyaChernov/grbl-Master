namespace grbl.Master.Service.Implementation
{
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Reactive;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;

    public class GrblStatusRequester : IGrblStatusRequester
    {
        private readonly StatusCommandSender _systemCommandSender;
        readonly Subject<Unit> _stopSubject = new Subject<Unit>();
        private TimeSpan _interval = TimeSpan.Zero;

        public GrblStatusRequester(ICommandSender commandSender)
        {
            _systemCommandSender = new StatusCommandSender(commandSender);
            _systemCommandSender.CommandFinished += SystemCommandSenderCommandFinished;
        }

        private void Request()
        {
            _systemCommandSender.Send("?");
        }

        private async void SystemCommandSenderCommandFinished(object sender, DataTypes.Command e)
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

        public bool IsRunning
        {
            get;
            set;
        }
    }
}

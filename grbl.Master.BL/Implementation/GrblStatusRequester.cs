namespace grbl.Master.BL.Implementation
{
    using System;
    using System.Reactive;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;

    using grbl.Master.BL.Interface;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;

    public class GrblStatusRequester : IGrblStatusRequester
    {
        private readonly ICommandSender _commandSender;
        readonly Subject<Unit> _stopSubject = new Subject<Unit>();
        private TimeSpan _interval = TimeSpan.Zero;

        public GrblStatusRequester(ICommandSender commandSender)
        {
            this._commandSender = commandSender;
            commandSender.CommandFinished += this.SystemCommandSenderCommandFinished;
        }

        public bool IsRunning
        {
            get;
            set;
        }

        private void Request()
        {
            this._commandSender.Send("?", CommandType.StatusRequest);
        }

        private async void SystemCommandSenderCommandFinished(object sender, Service.DataTypes.Command e)
        {
            if (this.IsRunning && e.Type == CommandType.StatusRequest)
            {
                await Task.Delay(this._interval);
                this.Request();
            }
        }

        public void StartRequesting(TimeSpan interval)
        {
            this._interval = interval;
            this.IsRunning = true;
            this.Request();
        }

        public void StopRequesting()
        {
            this.IsRunning = false;
            this._stopSubject.OnNext(Unit.Default);
        }
    }
}

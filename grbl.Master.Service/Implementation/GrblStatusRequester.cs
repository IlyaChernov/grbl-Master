namespace grbl.Master.Service.Implementation
{
    using grbl.Master.Service.Interface;
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public class GrblStatusRequester : IGrblStatusRequester
    {
        private ICommandSender _commandSender;
        private readonly SystemCommandSender _systemCommandSender;
        readonly Subject<Unit> _stopSubject = new Subject<Unit>();

        public GrblStatusRequester(ICommandSender commandSender)
        {
            _commandSender = commandSender;
            _systemCommandSender = new SystemCommandSender(commandSender);
        }

        public void StartRequesting(TimeSpan interval)
        {
            IsRunning = true;
            Observable.Timer(TimeSpan.Zero, interval).TakeUntil(_stopSubject).Subscribe(
                l =>
                    {
                        _systemCommandSender.Send("?");
                    });

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

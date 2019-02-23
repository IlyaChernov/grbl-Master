namespace grbl.Master.BL.Implementation
{
    using grbl.Master.BL.Interface;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class GrblStatusRequester : IGrblStatusRequester
    {
        private readonly ICommandSender _commandSender;
        private TimeSpan _interval = TimeSpan.Zero;
        private bool _executing;

        CancellationTokenSource source = new CancellationTokenSource();

        public GrblStatusRequester(ICommandSender commandSender, IGrblPrompt grblPrompt, IGrblStatusProcessor grblStatusProcessor)
        {
            _commandSender = commandSender;
            grblPrompt.PromptReceived += GrblPromptPromptReceived;
            grblStatusProcessor.StatusReceived += GrblStatusProcessorStatusReceived;
        }

        private async void GrblStatusProcessorStatusReceived(object sender, EventArgs e)
        {
            if (IsRunning && !_executing)
            {
                _executing = true;
                await Task.Delay(_interval, source.Token);
                Request();
                _executing = false;
            }
        }

        private void GrblPromptPromptReceived(object sender, string e)
        {
            StartRequesting(TimeSpan.FromMilliseconds(200));
        }

        public bool IsRunning
        {
            get; private set;
        }

        private void Request()
        {
            _commandSender.Send("?", CommandType.Realtime);
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
            source.Cancel();
        }
    }
}

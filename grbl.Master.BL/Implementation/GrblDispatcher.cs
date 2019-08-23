﻿namespace grbl.Master.BL.Implementation
{
    using grbl.Master.BL.Interface;
    using grbl.Master.Service.Enum;
    using grbl.Master.Service.Interface;
    using System;

    public class GrblDispatcher : IGrblDispatcher
    {
        private readonly IGrblStatus _grblStatus;

        private readonly ICommandSender _commandSender;

        public GrblDispatcher(IComService comService, IGrblPrompt grblPrompt, IGrblStatus grblStatus, ICommandSender commandSender)
        {
            _commandSender = commandSender;
            _grblStatus = grblStatus;
            grblPrompt.PromptReceived += GrblPromptPromptReceived;
            comService.ConnectionStateChanged += ComServiceConnectionStateChanged;
        }

        private void ComServiceConnectionStateChanged(object sender, ConnectionState e)
        {
            if (e == ConnectionState.Offline)
            {
                _grblStatus.StopRequesting();
                _commandSender.PurgeQueues();
            }
        }

        private void GrblPromptPromptReceived(object sender, string e)
        {
            _grblStatus.StartRequesting(TimeSpan.FromMilliseconds(200));
        }
    }
}
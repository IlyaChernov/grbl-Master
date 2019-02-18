using Caliburn.Micro;
using grbl.Master.UI.Input;
using grbl.Master.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace grbl.Master.UI
{
    using grbl.Master.BL.Implementation;
    using grbl.Master.BL.Interface;
    using grbl.Master.Service.Implementation;
    using grbl.Master.Service.Interface;

    public class Bootstrapper : BootstrapperBase
    {
        private readonly SimpleContainer _container = new SimpleContainer();

        protected override object GetInstance(Type serviceType, string key)
        {
            var instance = _container.GetInstance(serviceType, key);
            if (instance != null)
                return instance;
            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetAllInstances(serviceType);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }


        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MasterViewModel>();
        }

        protected override void Configure()
        {
            base.Configure();

            _container.RegisterSingleton(typeof(IComService), null, typeof(COMService));
            _container.RegisterSingleton(typeof(ICommandSender), null, typeof(CommandSender));
            _container.RegisterSingleton(typeof(IGrblStatusRequester), null, typeof(GrblStatusRequester));

            _container.RegisterSingleton(typeof(IGrblStatusProcessor), null, typeof(GrblStatusProcessor));


            _container.RegisterPerRequest(typeof(IWindowManager), null, typeof(WindowManager));
            _container.RegisterPerRequest(typeof(MasterViewModel), null, typeof(MasterViewModel));
            _container.RegisterPerRequest(typeof(COMConnectionViewModel), "COMConnectionViewModel", typeof(COMConnectionViewModel));

            var defaultCreateTrigger = Parser.CreateTrigger;

            Parser.CreateTrigger = (target, triggerText) =>
            {
                if (triggerText == null)
                {
                    return defaultCreateTrigger(target, null);
                }

                var triggerDetail = triggerText
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty);

                var splits = triggerDetail.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                switch (splits[0])
                {
                    case "Key":
                        var key = (Key)Enum.Parse(typeof(Key), splits[1], true);
                        return new KeyTrigger { Key = key };

                    case "Gesture":
                        var mkg = (MultiKeyGesture)(new MultiKeyGestureConverter()).ConvertFrom(splits[1]);

                        if (mkg != null)
                        {
                            return new KeyTrigger { Modifiers = mkg.KeySequences[0].Modifiers, Key = mkg.KeySequences[0].Keys[0] };
                        }

                        break;
                }

                return defaultCreateTrigger(target, triggerText);
            };
        }
    }
}

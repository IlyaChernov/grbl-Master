using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using grbl.Master.Communication;
using grbl.Master.UI.ViewModels;

namespace grbl.Master.UI
{
    public class Bootstrapper : BootstrapperBase
    {
        private SimpleContainer _container = new SimpleContainer();

        protected override object GetInstance(Type serviceType, string key)
        {
            //return _container.GetInstance(serviceType, key);
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
            _container.RegisterSingleton(typeof(ICOMService), null, typeof(COMService));
            _container.RegisterPerRequest(typeof(IWindowManager), null, typeof(WindowManager));
            _container.RegisterPerRequest(typeof(MasterViewModel), null, typeof(MasterViewModel));
            _container.RegisterPerRequest(typeof(COMConnectionViewModel), "COMConnectionViewModel", typeof(COMConnectionViewModel));
        }
    }
}

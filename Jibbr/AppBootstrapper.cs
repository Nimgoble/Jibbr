using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Jibbr.ViewModels;

namespace Jibbr
{
    public class AppBootstrapper : Bootstrapper<ShellViewModel>
    {
        private readonly SimpleContainer _container = new SimpleContainer();

        protected override void Configure()
        {
            Execute.InitializeWithDispatcher();
            _container.Instance<IWindowManager>(new WindowManager());
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.PerRequest<ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }
        
    }
}

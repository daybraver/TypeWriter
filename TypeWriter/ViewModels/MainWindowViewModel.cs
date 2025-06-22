using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeWriter.PubSubEvents;
using TypeWriter.Services;

namespace TypeWriter.ViewModels
{
    internal class MainWindowViewModel
    {
        private readonly AppConfigSource _appConfigSource;
        private readonly IEventAggregator _eventAggregator;
        private AppConfig _appConfig;

        public MainWindowViewModel(AppConfigSource appConfigSource,IEventAggregator eventAggregator)
        {
            _appConfigSource = appConfigSource;
            _eventAggregator = eventAggregator;
            _appConfig = _appConfigSource.GetConfig().Clone() as AppConfig;
        }

        public AppConfig AppConfig
        {
            get => _appConfig;
        }

        public void Save()
        {
            _appConfigSource.SaveConfig(_appConfig);
            _eventAggregator.GetEvent<AppConfigChangedEvent>().Publish();
        }
    }
}

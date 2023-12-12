using LogReader_WPF.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogReader_WPF.Repository
{
    public class SettingsRepository
    {
        private readonly IConfiguration _config;

        public SettingsRepository(IConfiguration config)
        {
            _config = config;
        }

        public SettingsModel GetSettings()
        {
            SettingsModel settingModel = _config.Get<SettingsModel>();

            return settingModel;
        }

        public T ConfigSetting<T>(string settingName)
        {

            Type ModelType = Activator.CreateInstance<T>().GetType();

            var ConfigVales = _config.GetSection(settingName).Get(ModelType);

            return (T)Convert.ChangeType(ConfigVales, typeof(T));
        }

        
    }
}

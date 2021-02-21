using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Infrastructure
{
    public class ConfigProviderService : IAppConfig
    {
        private AppCfgClass _appcfg;

        public AppCfgClass AppCfg { get => _appcfg; }

        public void SetAppCfg(AppCfgClass appCfg) {

            _appcfg = appCfg;
        }
    }


    public interface IAppConfig
    {
        public AppCfgClass AppCfg { get; }
    }
        
}

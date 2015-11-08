using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CobaltSettings.Elements;
using CobaltSettings.Serializers;

namespace CobaltSettings
{
    public class Settings
    {
        private ISettingsSerializer _serializer;
        public Settings(ISettingsSerializer serializer)
        {
            _serializer = serializer;
        }

        public SettingsElement RootElement
        {
            get;
            set;
        }

        public bool Save()
        {
            throw new NotImplementedException();
        }

        public bool Load()
        {
            throw new NotImplementedException();
        }
    }
}

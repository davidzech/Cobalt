using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cobalt.Settings.Elements;
using Cobalt.Settings.Serializers;

namespace Cobalt.Settings
{
    [Export(typeof(ISettings))]
    public class Settings : ISettings
    {
        private readonly ISettingsSerializer _serializer;
        private readonly string _settingsFileName;
        public Settings(ISettingsSerializer serializer, string settingsFileName = "settings")
        {
            this.RootElement = new SettingsElement();
            _serializer = serializer;
            _settingsFileName = settingsFileName;
        }

        public SettingsElement RootElement
        {
            get;
            set;
        }

        public void Save()
        {
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }
            File.WriteAllText(SettingsFilePath, _serializer.Serialize(RootElement));
        }

        public string BasePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cobalt2");
#if DEBUG
        public string SettingsFilePath => Path.Combine(BasePath, $"{_settingsFileName}.DEBUG.{_serializer.GetFileExtension().ToLower()}");
#else
        public string SettingsFilePath => Path.Combine(BasePath, $"{_settingsFileName}{_serializer.GetFileExtension().ToLower()}");
#endif
        public async Task SaveAsync()
        {
            await Task.Run(() =>
            {
                this.Save();
            });
        }

        public void InitializeDefaults(bool overwrite = false)
        {
            if (Directory.Exists(BasePath))
            {
                if (overwrite == false && File.Exists(SettingsFilePath))
                {
                    return;
                }
            }
            this.RootElement = new SettingsElement()
            {
                Networks = 
                {
                    new NetworkElement()
                    {
                        Name = "Cobalt",
                        Hostname = "irc.cobaltapp.net",
                        Port = 6667,
                        Channels = {new ChannelElement()
                        {
                            Name = "#flux"                            
                        }}
                    }
                },
                DefaultProfile = new UserProfileElement()
                {
                    Nickname1 = $"{Environment.UserName}",
                    Nickname2 = $"{Environment.UserName}_",
                    Username = Environment.UserName,
                    FullName = Environment.UserName,
                    NickservPassword = "daki123"
                },
                FontFamily = "Segoe UI",
                FontSize = 14,
                ScrollbackLines = 300
            };
            Save();
        }

        public void Load()
        {
            try
            {
                string data = File.ReadAllText(SettingsFilePath);
                RootElement = _serializer.Deserialize(data);
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                InitializeDefaults();
                Save();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cobalt.Settings.Elements;
using Cobalt.Settings.Serializers;

namespace Cobalt.Settings
{
    public class Settings
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

        public void InitializeDefaults()
        {
            if (Directory.Exists(BasePath))
            {
                if (File.Exists(SettingsFilePath))
                {
                    return;
                }
            }
            this.RootElement = new SettingsElement()
            {
                Servers = new List<ServerElement>()
                {
                    new ServerElement()
                    {
                        Name = "Cobalt",
                        Hostname = "irc.cobaltapp.net",
                        Port = 6667,
                        Channels = new List<string>() {"#flux"}
                    }
                },
                DefaultProfile = new UserProfileElement()
                {
                    Nickname1 = $"{Environment.UserName}",
                    Nickname2 = $"{Environment.UserName}_",
                    Nickname3 = $"{Environment.UserName}__",
                    Username = Environment.UserName,
                    FullName = Environment.UserName
                },
                FontFamily = "Segoe UI",
                FontSize = 14,
                ScrollbackLines = 300
            };
            Save();
        }

        public void Load()
        {
            string data = File.ReadAllText(SettingsFilePath);
            RootElement = _serializer.Deserialize(data);
        }
    }
}

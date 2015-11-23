using System.Threading.Tasks;
using Cobalt.Settings.Elements;

namespace Cobalt.Settings
{
    public interface ISettings
    {
        SettingsElement RootElement { get; set; }
        string BasePath { get; }
        string SettingsFilePath { get; }
        void Save();
        Task SaveAsync();
        void InitializeDefaults(bool overwrite = false);
        void Load();
    }
}
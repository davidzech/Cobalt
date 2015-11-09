using System.Threading.Tasks;
using Cobalt.Settings.Elements;

namespace Cobalt.Settings.Serializers
{
    public interface ISettingsSerializer
    {
        string Serialize(SettingsElement rootElement);

        Task<string> SerializeAsync(SettingsElement rootElement);

        SettingsElement Deserialize(string data);

        Task<SettingsElement> DeserializeAsync(string filePath);
    }
}
using System.Threading.Tasks;
using CobaltSettings.Elements;

namespace CobaltSettings.Serializers
{
    public interface ISettingsSerializer
    {
        string Serialize(SettingsElement rootElement);

        Task<string> SerializeAsync(SettingsElement rootElement);

        SettingsElement Deserialize(string data);

        Task<SettingsElement> DeserializeAsync(string filePath);
    }
}
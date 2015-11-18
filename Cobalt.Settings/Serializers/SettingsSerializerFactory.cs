using System;

namespace Cobalt.Settings.Serializers
{
    public static class SettingsSerializerFactory
    {
        public static ISettingsSerializer Get(string serializer)
        {
            switch (serializer.ToUpper())
            {
                case "JSON":
                    return new JsonSettingsSerializer();
                case "XML":
                    return new XmlSettingsSerializer();
                default:
                    throw new ArgumentException("Invalid serializer type", nameof(serializer));
            }
        }
    }
}

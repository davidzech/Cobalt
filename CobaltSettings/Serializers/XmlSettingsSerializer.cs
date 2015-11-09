using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Cobalt.Settings.Elements;

namespace Cobalt.Settings.Serializers
{
    internal class XmlSettingsSerializer : ISettingsSerializer
    {
        public string Serialize(SettingsElement rootElement)
        {
            XmlSerializer x = new XmlSerializer(typeof(SettingsElement));
            using (StringWriter sw = new StringWriter())
            {
                x.Serialize(sw, rootElement);
                return sw.ToString();
            }
        }

        public Task<string> SerializeAsync(SettingsElement rootElement)
        {
            throw new NotImplementedException();
        }

        public SettingsElement Deserialize(string data)
        {
            throw new NotImplementedException();
        }

        public Task<SettingsElement> DeserializeAsync(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}

using System.Linq;
using Cobalt.Settings.Elements;
using Cobalt.Settings.Serializers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CobaltSettingsTests
{
    [TestClass]
    public class JsonTests
    {
        private SettingsElement createTestElement()
        {
            SettingsElement root = new SettingsElement
            {
                ScrollbackLines = 300,
                FontFamily = "Segoe UI",
                FontSize = 14,
                DefaultProfile = new UserProfileElement()
                {
                    Nickname1 = "Nick1",
                    FullName = "FullName",
                    Username = "Username"
                }
            };
            root.Servers.Add(new ServerElement()
            {
                Name = "Memers",
                Port = 6667,
                ProfileOverride = new UserProfileElement()
                {
                    Nickname1 = "Override",
                    FullName = "Override",
                    Username = "Override"
                }
            });

            return root;
        }

        private static readonly string Expected =
            "{\"FontFamily\":\"Segoe UI\",\"FontSize\":14,\"ScrollbackLines\":300,\"DefaultProfile\":{\"Nickname1\":\"Nick1\",\"Nickname2\":null,\"Nickname3\":null,\"Username\":\"Username\",\"FullName\":\"FullName\",\"NickservPassword\":null},\"Servers\":[{\"Name\":\"Memers\",\"Port\":6667,\"Password\":null,\"Channels\":[],\"IsSecure\":false,\"ConnectOnStartup\":false,\"AutoReconnect\":false,\"ProfileOverride\":{\"Nickname1\":\"Override\",\"Nickname2\":null,\"Nickname3\":null,\"Username\":\"Override\",\"FullName\":\"Override\",\"NickservPassword\":null}}]}";

        [TestMethod]
        public void TestSerialize()
        {
            ISettingsSerializer json = SettingsSerializerFactory.Get("JSON");
            SettingsElement root = createTestElement();
            
            string output = json.Serialize(root);
            Assert.IsTrue(Expected == output);
        }

        [TestMethod]
        public void TestDeserialize()
        {
            ISettingsSerializer json = SettingsSerializerFactory.Get("JSON");
            SettingsElement root = createTestElement();
                        
            SettingsElement deserialized = json.Deserialize(Expected);

            Assert.IsTrue(deserialized.FontFamily == root.FontFamily);
            Assert.IsTrue(deserialized.ScrollbackLines == root.ScrollbackLines);
            Assert.IsTrue(deserialized.DefaultProfile.FullName == root.DefaultProfile.FullName);
            Assert.IsTrue(deserialized.Servers.First().ProfileOverride.FullName ==
                          root.Servers.First().ProfileOverride.FullName);
        }

    }
}
using System.Linq;
using Cobalt.Settings.Elements;
using Cobalt.Settings.Serializers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cobalt.Settings.Tests
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
            root.Networks.Add(new NetworkElement()
            {
                Name = "Memers",
                Port = 6667,
                UserProfile = new UserProfileElement()
                {
                    Nickname1 = "Override",
                    FullName = "Override",
                    Username = "Override"
                }
            });

            return root;
        }

        private static readonly string Expected =
            "{\"FontFamily\":\"Segoe UI\",\"FontSize\":14,\"ScrollbackLines\":300,\"DefaultProfile\":{\"Nickname1\":\"Nick1\",\"Nickname2\":null,\"Nickname3\":null,\"Username\":\"Username\",\"FullName\":\"FullName\",\"NickservPassword\":null},\"Networks\":[{\"Name\":\"Memers\",\"Hostname\":\"\",\"Port\":6667,\"Password\":null,\"Channels\":[],\"IsSecure\":false,\"ConnectOnStartup\":false,\"AutoReconnect\":false,\"UserProfile\":{\"Nickname1\":\"Override\",\"Nickname2\":null,\"Nickname3\":null,\"Username\":\"Override\",\"FullName\":\"Override\",\"NickservPassword\":null}}]}";
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
            Assert.IsTrue(deserialized.Networks.First().UserProfile.FullName ==
                          root.Networks.First().UserProfile.FullName);
        }

    }
}
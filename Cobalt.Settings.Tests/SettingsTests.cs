using System;
using System.IO;
using Cobalt.Settings;
using Cobalt.Settings.Elements;
using Cobalt.Settings.Serializers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CobaltSettingsTests
{
    [TestClass]
    public class SettingsTests
    {
        [TestMethod]
        public void TestSave()
        {
            Settings s = new Settings(SettingsSerializerFactory.Get("JSON"), "settings.test");
            s.Save();
            File.Delete(s.SettingsFilePath);
        }

        [TestMethod]
        public void TestDefaultInitialize()
        {
            Settings s = new Settings(SettingsSerializerFactory.Get("JSON"), "settings.test");
            s.InitializeDefaults(true);
            Assert.IsTrue(s.RootElement.ScrollbackLines == 300);
            File.Delete(s.SettingsFilePath);
        }
        
        [TestMethod]
        public void TestLoad()
        {
            Settings s = new Settings(SettingsSerializerFactory.Get("JSON"), "settings.test");
            s.InitializeDefaults();
            s.RootElement = null;
            s.Load();
            Assert.IsTrue(s.RootElement.ScrollbackLines == 300);
        }
    }
}

using System.Collections.Generic;
using Caliburn.Micro;
using Cobalt.Core.Irc;
using Cobalt.Settings;
using Cobalt.Settings.Elements;
using Cobalt.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Cobalt.Tests
{
    public class StubbedWindowManager : IWindowManager
    {
        public bool? ShowDialog(object rootModel, object context = null, IDictionary<string, object> settings = null)
        {
            return true;
        }

        public void ShowWindow(object rootModel, object context = null, IDictionary<string, object> settings = null)
        {
            
        }

        public void ShowPopup(object rootModel, object context = null, IDictionary<string, object> settings = null)
        {
            
        }
    }

    [TestClass]
    public class ShellTest
    {
        [TestInitialize]
        public void Setup()
        {
        }

        [TestMethod]
        public void TestShell()
        {
            var mockCoordinator = new Mock<IDialogCoordinator>();
            ISettings settings = new Settings.Settings(Settings.Serializers.SettingsSerializerFactory.Get("JSON"), "settings.TEST");
            settings.InitializeDefaults();
            ShellViewModel svm = new ShellViewModel(new StubbedWindowManager(), mockCoordinator.Object, settings);
            var mock = new Mock<IrcConnection>();

            svm.ActivateItem(new IrcServerTabViewModel(settings, mock.Object) {DisplayName = "Test"});
            Assert.IsTrue(svm.Tabs.Count == 1);
    }
        }
}

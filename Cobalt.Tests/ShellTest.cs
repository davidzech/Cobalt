using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Cobalt.Extensibility;
using Cobalt.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        [TestMethod]
        public void TestShell()
        {
            ShellViewModel svm = new ShellViewModel(new StubbedWindowManager());
            svm.ActivateItem(new IrcTabViewModel() {DisplayName = "Test"});
            Assert.IsTrue(svm.Tabs.Count == 1);
        }
    }
}

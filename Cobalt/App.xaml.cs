using System;
using System.Linq;
using System.Windows;
using MahApps.Metro;

namespace Cobalt
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {

        public App()
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            try
            {
                Settings =
                    new Cobalt.Settings.Settings(Cobalt.Settings.Serializers.SettingsSerializerFactory.Get("JSON"),
                        "settings");
                Settings.Load();
            }
            catch (Exception e)
            {
                MessageBox.Show("An issue occured when loading application settings. Please try removing the settings file.");                
            }

        }

        public static Settings.Settings Settings
        {
            get;
            private set;
        } 
    }
}

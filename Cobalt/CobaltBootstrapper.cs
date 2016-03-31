using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Cobalt.ViewModels;
using Caliburn.Micro;
using Cobalt.Core.Network;
using Cobalt.Extensibility;
using Cobalt.Settings;
using MahApps.Metro.Controls.Dialogs;
using ThemeManager = MahApps.Metro.ThemeManager;

namespace Cobalt
{
    public class CobaltBootstrapper : MetroBootstrapper<IShell>
    {
        public CobaltBootstrapper()
        {
            Initialize();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            IoC.Get<ISettings>().Save();
            base.OnExit(sender, e);
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            var startupTasks =
                GetAllInstances(typeof(StartupTask))
                .Cast<ExportedDelegate>()
                .Select(exportedDelegate => (StartupTask)exportedDelegate.CreateDelegate(typeof(StartupTask)));

            startupTasks.Apply(s => s());
            base.OnStartup(sender, e);
            ThemeManager.AddAppTheme("CobaltLight", new Uri("pack://application:,,,/Themes/CobaltLight.xaml"));
            ThemeManager.AddAppTheme("CobaltDark", new Uri("pack://application:,,,/Themes/CobaltDark.xaml"));
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.Accents.First(a => a.Name == "Blue"),
                ThemeManager.GetAppTheme("CobaltLight"));
            await NatHelper.DiscoverAsync();
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            //LogUnhandledException
            MessageBox.Show(
                $"{e.Exception.Message}  {Environment.NewLine + Environment.NewLine} The Application will now terminate.",
                "An Unrecoverable Error has Occured", MessageBoxButton.OK, MessageBoxImage.Error);
            try
            {
                IoC.Get<ISettings>().Save();
            }
            catch (Exception)
            {
                MessageBox.Show("Changes since last launch will be lost", "Unable to save settings", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void ConfigureContainer(CompositionBatch builder)
        {            
            base.ConfigureContainer(builder);
            builder.AddExportedValue<IWindowManager>(new CobaltWindowManager());
            builder.AddExportedValue<IDialogCoordinator>(new MetroDialogManager());
            builder.AddExportedValue<IEventAggregator>(new EventAggregator());
            var settings = new Settings.Settings(Cobalt.Settings.Serializers.SettingsSerializerFactory.Get("JSON"),
                        "settings");
            settings.Load();
            builder.AddExportedValue<ISettings>(settings);
        }

    }
}

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
using ThemeManager = MahApps.Metro.ThemeManager;

namespace Cobalt
{
    public class CobaltBootstrapper : MetroBootstrapper<IShell>
    {
        public CobaltBootstrapper()
        {
            Initialize();
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            var startupTasks =
                GetAllInstances(typeof(StartupTask))
                .Cast<ExportedDelegate>()
                .Select(exportedDelegate => (StartupTask)exportedDelegate.CreateDelegate(typeof(StartupTask)));

            startupTasks.Apply(s => s());            
            base.OnStartup(sender, e);
            await NatHelper.DiscoverAsync();
            ThemeManager.AddAppTheme("CobaltLight", new Uri("pack://application:,,,/Themes/CobaltLight.xaml"));
            ThemeManager.AddAppTheme("CobaltDark", new Uri("pack://application:,,,/Themes/CobaltDark.xaml"));
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            //LogUnhandledException
            MessageBox.Show(
                $"{e.Exception.Message}  {Environment.NewLine + Environment.NewLine} The Application will now terminate.",
                "An Unrecoverable Error has Occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void ConfigureContainer(CompositionBatch builder)
        {            
            base.ConfigureContainer(builder);
            builder.AddExportedValue<IWindowManager>(new CobaltWindowManager());
            builder.AddExportedValue<IEventAggregator>(new EventAggregator());
        }

    }
}

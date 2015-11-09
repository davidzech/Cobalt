using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Cobalt.ViewModels;
using CobaltCore.Network;
using MahApps.Metro;

namespace Cobalt
{
    public class CobaltBootstrapper : BootstrapperBase
    {
        public CobaltBootstrapper()
        {
            Initialize();
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
            await NatHelper.DiscoverAsync();
            ThemeManager.AddAppTheme("CobaltLight", new Uri("pack://application:,,,/Themes/CobaltLight.xaml"));
            ThemeManager.AddAppTheme("CobaltDark", new Uri("pack://application:,,,/Themes/CobaltDark.xaml"));

        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            //LogUnhandledException
            MessageBox.Show(
                $"{((Exception)e.Exception).Message}  {Environment.NewLine + Environment.NewLine} The Application will now terminate.",
                "An Unrecoverable Error has Occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

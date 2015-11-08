using System;
using System.Windows;
using CobaltCore.Network;

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
            AppDomain.CurrentDomain.UnhandledException += CaptureUnhandledException;
            Startup += App_Startup;
            Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
#warning Finish this
            //App.Settings.Save()
        }

        private static void CaptureUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //LogUnhandledException
            MessageBox.Show(
                $"{((Exception)e.ExceptionObject).Message}  {Environment.NewLine + Environment.NewLine} The Application will now terminate.",
                "An Unrecoverable Error has Occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async void App_Startup(object sender, StartupEventArgs e)
        {
            await NatHelper.DiscoverAsync();
        }
        

        
    }
}

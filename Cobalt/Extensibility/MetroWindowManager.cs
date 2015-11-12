using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using MahApps.Metro.Controls;

namespace Cobalt.Extensibility
{
    public abstract class MetroWindowManager : WindowManager
    {
        private ResourceDictionary[] _resourceDictionaries;

        public virtual void ConfigureWindow(MetroWindow window)
        {

        }
        public virtual MetroWindow CreateCustomWindow(object view, bool windowIsView)
        {
            MetroWindow result;
            if (windowIsView)
            {
                result = view as MetroWindow;
            }
            else
            {
                result = new MetroWindow
                {
                    Content = view
                };
            }

            AddMetroResources(result);
            return result;
        }

        private void AddMetroResources(MetroWindow window)
        {
            _resourceDictionaries = LoadResources();
            foreach (ResourceDictionary dictionary in _resourceDictionaries)
            {
                window.Resources.MergedDictionaries.Add(dictionary);
            }
        }

        private ResourceDictionary[] LoadResources()
        {
            return new[]
                       {
                           new ResourceDictionary
                               {
                                   Source =
                                       new Uri(
                                       "pack://application:,,,/MahApps.Metro;component/Styles/Colors.xaml",
                                       UriKind.RelativeOrAbsolute)
                               },
                           new ResourceDictionary
                               {
                                   Source =
                                       new Uri(
                                       "pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml",
                                       UriKind.RelativeOrAbsolute)
                               },
                           new ResourceDictionary
                               {
                                   Source =
                                       new Uri(
                                       "pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml",
                                       UriKind.RelativeOrAbsolute)
                               },
                           new ResourceDictionary
                               {
                                   Source =
                                       new Uri(
                                       "pack://application:,,,/MahApps.Metro;component/Styles/Controls.AnimatedSingleRowTabControl.xaml",
                                       UriKind.RelativeOrAbsolute)
                               },
                           new ResourceDictionary
                               {
                                   Source =
                                       new Uri(
                                       "pack://application:,,,/Themes/CobaltLight.xaml",
                                       UriKind.RelativeOrAbsolute)
                               },
                           new ResourceDictionary
                               {
                                   Source =
                                       new Uri(
                                       "pack://application:,,,/MahApps.Metro;component/Styles/Accents/Blue.xaml",
                                       UriKind.RelativeOrAbsolute)
                               }

                       };
        }
    }
}

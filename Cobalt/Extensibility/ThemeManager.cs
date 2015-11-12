using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace Cobalt.Extensibility
{
    [Export(typeof(IThemeManager))]
    public class ThemeManager : IThemeManager
    {
        private readonly ResourceDictionary _themeResources;

        public ThemeManager()
        {
            _themeResources = new ResourceDictionary
                                      {
                                          Source =
                                              new Uri("pack://application:,,,/Themes/CobaltMetro.xaml")
                                      };
        }

        public ResourceDictionary GetThemeResources()
        {
            return _themeResources;
        }
    }
}
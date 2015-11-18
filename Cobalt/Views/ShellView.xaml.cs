using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Cobalt.Behaviors;
using Cobalt.Core.Irc;

namespace Cobalt.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView
    {
        public ShellView()
        {
            InitializeComponent();
        }

        private void Channels_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is ToggleButton))
                e.Handled = true;
        }

        private void Channels_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var tvi = sender as TreeViewItem;
            tvi?.Focus();
        }
    }
}

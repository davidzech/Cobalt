using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Cobalt.Settings.Elements;

namespace Cobalt.Views.Flyouts
{
    /// <summary>
    /// Interaction logic for ServersFlyoutView.xaml
    /// </summary>
    public partial class NetworksFlyoutView : UserControl
    {
        public NetworksFlyoutView()
        {
            InitializeComponent();
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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

namespace Cobalt.Controls
{
    /// <summary>
    /// Interaction logic for ChatBox.xaml
    /// </summary>
    public partial class ChatBox : UserControl
    {
        public ChatBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MessagesSourceProperty = DependencyProperty.Register(
            "MessagesSource", typeof (IEnumerable), typeof (ChatBox));

        public IEnumerable MessagesSource
        {
            get { return (IEnumerable)GetValue(MessagesSourceProperty); }
            set { SetValue(MessagesSourceProperty, value); }
        }      
    }
}

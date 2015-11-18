using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls;

namespace Cobalt.ViewModels.Flyouts
{
    public class ServersFlyoutViewModel : FlyoutViewModelBase
    {
        public override string Header
        {
            get { return "Servers"; }
            set
            {
            }
        }

        public override Position Position
        {
            get
            {
                return Position.Left;
            }
            set
            {
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;

namespace Cobalt.Extensibility
{
    [Export(typeof(IDialogCoordinator))]
    public class MetroDialogManager : DialogCoordinator
    {
    }
}

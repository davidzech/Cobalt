using System.ComponentModel.Composition;
using Cobalt.Extensibility;
using Caliburn.Micro;

namespace Cobalt.Extensibility
{
    [Export(typeof(IWindowManager))]
    public class CobaltWindowManager : MetroWindowManager 
    {

    }
}

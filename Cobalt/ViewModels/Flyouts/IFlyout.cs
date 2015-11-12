using Caliburn.Micro;
using MahApps.Metro.Controls;

namespace Cobalt.ViewModels.Flyouts
{
    public interface IFlyout : IScreen
    {
        string Header { get; set; }

        bool IsOpen { get; set; }

        Position Position { get; set; }
    }
}
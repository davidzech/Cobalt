using Caliburn.Micro;
using MahApps.Metro.Controls;

namespace Cobalt.ViewModels.Flyouts
{   
    public abstract class FlyoutViewModelBase : Screen, IFlyout
    {
        private string _header;
        public virtual string Header
        {
            get { return _header; }
            set
            {
                if (_header != value)
                {
                    _header = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        private bool _isOpen;

        public virtual bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (_isOpen != value)
                {
                    _isOpen = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        private Position _position;
        public virtual Position Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    NotifyOfPropertyChange();
                }
            }
        }
    }
}

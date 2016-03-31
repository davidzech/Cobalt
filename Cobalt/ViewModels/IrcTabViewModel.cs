using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Cobalt.Controls;
using Cobalt.Core.Irc;

namespace Cobalt.ViewModels
{
    public partial class IrcTabViewModel : Screen
    {
        public IrcConnection Connection { get; }

        public IObservableCollection<IrcTabViewModel> Children { get; } = new BindableCollection<IrcTabViewModel>();
        public IObservableCollection<MessageLine> Messages { get; } = new BindableCollection<MessageLine>();

        public IrcTabViewModel(IrcConnection connection)
        {
            Connection = connection;            
            SubscribeIrcEvents();
        }

        public void AddChild(IrcTabViewModel child)
        {
            child.ParentTab = this;
            Children.Add(child);
        }

        private IrcTabViewModel _parentTab;

        public IrcTabViewModel ParentTab
        {
            get { return _parentTab; }
            set
            {
                _parentTab = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isExpanded = true;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _autoJoin;
        public bool AutoJoin
        {
            get { return _autoJoin; }
            set
            {
                _autoJoin = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                NotifyOfPropertyChange();
            }
        }

        private int _bufferLines = 100;

        public int BufferLines
        {
            get { return _bufferLines; }
            set
            {
                _bufferLines = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsChannel => ParentTab != null;

        public bool IsServer => !IsChannel;

        public void Disconnect()
        {
            MessageBox.Show("Disconnect");
        }

        public void Reconnect()
        {                        
            MessageBox.Show("Reconnect");
        }

        public void Close()
        {
            TryClose();
        }       
    }
}

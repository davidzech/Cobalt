using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Cobalt.Controls;
using Cobalt.Core.Irc;
using Cobalt.Settings;

namespace Cobalt.ViewModels
{
    public abstract partial class IrcTabViewModel : Screen
    {
        protected ISettings Settings { get; }
        protected IrcConnection Connection { get; }

        public string UniqueIdentifier { get; }
      
        public IObservableCollection<MessageLine> Messages { get; } = new BindableCollection<MessageLine>();

        [ImportingConstructor]
        protected IrcTabViewModel(ISettings settings, IrcConnection connection, string uniqueIdentifier = null)
        {
            Settings = settings;
            Connection = connection;
            UniqueIdentifier = uniqueIdentifier;
            SubscribeIrcEvents();
        }

        ~IrcTabViewModel()
        {
            UnsubscribeIrcEvents();
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

        private bool _isConnected = false;

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                NotifyOfPropertyChange();
            }
        }

        private int _bufferLines = 500;

        public int BufferLines
        {
            get { return _bufferLines; }
            set
            {
                _bufferLines = value;
                NotifyOfPropertyChange();
            }
        }

        public abstract bool IsChannel { get; }

        public abstract bool IsServer { get; }        

        public void Disconnect()
        {
            MessageBox.Show("Disconnect");
        }

        public void Reconnect()
        {                        
            MessageBox.Show("Reconnect");
        }

        public async void Close()
        {        
            TryClose();
            await Connection.QuitAsync("Leaving").ConfigureAwait(false);
        }      
    }
}

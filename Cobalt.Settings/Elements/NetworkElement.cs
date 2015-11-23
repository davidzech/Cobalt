using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Caliburn.Micro;
using CobaltSettings.Annotations;

namespace Cobalt.Settings.Elements
{
    [Serializable]
    public sealed class NetworkElement : INotifyPropertyChanged
    {
        private string _name = "";
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    if (value == "")
                    {
                        throw new ArgumentException($"{nameof(Name)} cannot be empty");
                    }
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _hostname = "";

        public string Hostname
        {
            get { return _hostname; }
            set
            {
                if (_hostname != value)
                {
                    _hostname = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _port = 6667;

        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
              if (value <= 0 || value > 65535)
                    throw new ArgumentException($"{nameof(Port)} must be between 1 and 65535");                
                if (_port != value)
                {
                    _port = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _password;

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                }
            }
        }

        readonly IObservableCollection<ChannelElement> _channels = new BindableCollection<ChannelElement>();
        public IObservableCollection<ChannelElement> Channels => _channels;

        private bool _isSecure;

        public bool IsSecure
        {
            get
            {
                return _isSecure;
            }
            set
            {
                if (_isSecure != value)
                {
                    _isSecure = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _connectOnStartup;

        public bool ConnectOnStartup
        {
            get { return _connectOnStartup; }
            set
            {
                if (_connectOnStartup != value)
                {
                    _connectOnStartup = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _autoreconnect;

        public bool AutoReconnect
        {
            get { return _autoreconnect; }
            set
            {
                if (_autoreconnect != value)
                {
                    _autoreconnect = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _overrideGlobalProfile = false;

        public bool OverrideGlobalProfile
        {
            get { return _overrideGlobalProfile; }
            set
            {
                if(_overrideGlobalProfile != value)
                {
                    _overrideGlobalProfile = value;
                    OnPropertyChanged();
                }
            }
        }

        private UserProfileElement _profileOverride = new UserProfileElement();

        public UserProfileElement UserProfile
        {
            get { return _profileOverride; }
            set
            {
                if (_profileOverride != value)
                {
                    _profileOverride = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _connectCommands;

        public string ConnectCommands
        {
            get { return _connectCommands; }
            set
            {
                if (_connectCommands != value)
                {
                    _connectCommands = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CobaltSettings.Annotations;

namespace Cobalt.Settings.Elements
{
    [Serializable]
    public sealed class ServerElement : INotifyPropertyChanged
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

        List<string> _channels = new List<string>();
        public List<string> Channels
        {
            get
            {
                return _channels;
            }
            set
            {
                if (_channels != value)
                {
                    _channels = value;                    
                    OnPropertyChanged();
                }
            }
        }

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

        private UserProfileElement _profileOverride;

        public UserProfileElement ProfileOverride
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CobaltSettings.Annotations;

namespace Cobalt.Settings.Elements
{
    [Serializable]    
    public class UserProfileElement : INotifyPropertyChanged
    {
        private string _nickname1;

        public string Nickname1
        {
            get { return _nickname1; }
            set
            {
                if (_nickname1 != value)
                {
                    _nickname1 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _nickname2;

        public string Nickname2
        {
            get { return _nickname2; }
            set
            {
                if (_nickname2 != value)
                {
                    _nickname2 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _nickname3;

        public string Nickname3
        {
            get { return _nickname3; }
            set
            {
                if (_nickname3 != value)
                {
                    _nickname3 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _username;

        public string Username
        {
            get { return _username; }
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _fullName;

        public string FullName
        {
            get { return _fullName; }
            set
            {
                if (_fullName != value)
                {
                    _fullName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _nickservPassword;

        public string NickservPassword
        {
            get { return _nickservPassword; }
            set
            {
                if (_nickservPassword != value)
                {
                    _nickservPassword = value;
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

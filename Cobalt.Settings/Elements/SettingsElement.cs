using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Caliburn.Micro;
using CobaltSettings.Annotations;

namespace Cobalt.Settings.Elements
{    
    [Serializable]
    public sealed class SettingsElement : INotifyPropertyChanged
    {
        private string _fontFamily;       
        public string FontFamily
        {
            get
            {                
                return _fontFamily;
            }
            set
            {
                if (!ReferenceEquals(value, _fontFamily))
                {
                    _fontFamily = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _fontSize;

        public int FontSize
        {
            get { return _fontSize; }
            set
            {
                if (_fontSize != value)
                {
                    _fontSize = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _scrollbackLines;
        public int ScrollbackLines
        {
            get { return _scrollbackLines; }
            set
            {
                if (_scrollbackLines != value)
                {
                    _scrollbackLines = value;
                    OnPropertyChanged();
                }
            }
        }
    
        private UserProfileElement _defaultProfile;

        public UserProfileElement DefaultProfile
        {
            get { return _defaultProfile; }
            set
            {
                if (_defaultProfile != value)
                {
                    _defaultProfile = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly IObservableCollection<NetworkElement> _networks = new BindableCollection<NetworkElement>();

        public IObservableCollection<NetworkElement> Networks => _networks;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

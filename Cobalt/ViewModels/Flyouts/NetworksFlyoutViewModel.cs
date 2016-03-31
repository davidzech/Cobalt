using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cobalt.Core.Irc;
using Cobalt.Settings;
using Cobalt.Settings.Elements;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Cobalt.ViewModels.Flyouts
{
    public sealed class NetworksFlyoutViewModel : FlyoutViewModelBase
    {
        public override string Header
        {
            get { return "Networks"; }
            set
            {
            }
        }

        private readonly IDialogCoordinator _coordinator;
        private readonly ISettings _settings;

        [ImportingConstructor]
        public NetworksFlyoutViewModel(IDialogCoordinator coordinator, ISettings settings)
        {
            _coordinator = coordinator;
            _settings = settings;
            SelectedNetwork = Networks.FirstOrDefault();
            PropertyChanged += NetworksFlyoutViewModel_PropertyChanged;
        }

        ~NetworksFlyoutViewModel()
        {
            PropertyChanged -= NetworksFlyoutViewModel_PropertyChanged;
        }

        public IObservableCollection<NetworkElement> Networks => _settings.RootElement.Networks;

        private NetworkElement _selectedNetwork;
        public NetworkElement SelectedNetwork
        {
            get { return _selectedNetwork; }
            set
            {
                if (_selectedNetwork != value)
                {
                    _selectedNetwork = value;
                    NotifyOfPropertyChange();
                }
            }            
        }

        private ChannelElement _selectedChannel;

        public ChannelElement SelectedChannel
        {
            get { return _selectedChannel; }
            set
            {
                if (_selectedChannel != value)
                {
                    _selectedChannel = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool CanAddChannel()
        {
            return _selectedNetwork != null;
        }

        public void AddChannel()
        {
            var add = new ChannelElement() {Name = "#"};
            _selectedNetwork?.Channels.Add(add);
            SelectedChannel = add;
        }

        public bool CanRemoveChannel()
        {
            return _selectedChannel != null;
        }

        public void RemoveChannel()
        {
            _selectedNetwork?.Channels?.Remove(_selectedChannel);
            _selectedChannel = _selectedNetwork?.Channels?.FirstOrDefault();
        }

        public void AddNetwork()
        {
            var network = new NetworkElement() { Name = "New Server"};        
            Networks.Add(network);
            SelectedNetwork = network;
        }

        public bool CanRemoveNetwork()
        {
            return SelectedNetwork != null;
        }

        public bool CanConnect()
        {
            return SelectedNetwork != null;
        }

        public async void Connect()
        {
            var svm = Parent as ShellViewModel;
            var connection = new IrcConnection();
            var chans = SelectedNetwork.Channels.Select(ce => new Tuple<string, string>(ce.Name, ce.Password));
            string[] commands = SelectedNetwork.ConnectCommands.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var server = new IrcServerTabViewModel(connection, chans, commands) {DisplayName = SelectedNetwork.Name};
            svm?.ActivateItem(server);
            IsOpen = false;
            var nickName = SelectedNetwork?.UserProfile?.Nickname1 ?? _settings.RootElement.DefaultProfile.Nickname1;
            var fullName = SelectedNetwork?.UserProfile?.FullName ?? _settings.RootElement.DefaultProfile.FullName;
            var userName = SelectedNetwork?.UserProfile?.Username ?? _settings.RootElement.DefaultProfile.Username;

            await connection.ConnectAsync(SelectedNetwork.Hostname, SelectedNetwork.Port, SelectedNetwork.IsSecure,
                    nickName, userName, fullName, SelectedNetwork.AutoReconnect, SelectedNetwork.Password);
        }

        private async void NetworksFlyoutViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsOpen))
            {
                await _settings.SaveAsync();
            }
        }

        public async void RemoveNetwork()
        {

            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Delete",
                NegativeButtonText = "Cancel"
            };

            var result = await _coordinator.ShowMessageAsync(Parent, "Remove Network?", $"Are you sure you want to remove the network: {SelectedNetwork.Name}?", MessageDialogStyle.AffirmativeAndNegative, mySettings);
            if (result == MessageDialogResult.Affirmative)
            {
                Networks.Remove(SelectedNetwork);
                SelectedNetwork = Networks.FirstOrDefault();
            }
        }

        public override Position Position
        {
            get
            {
                return Position.Left;
            }
            set
            {
            }
        }
    }
}
